

(function () {
    'use strict';

    angular
        .module('app')
        .controller('OrderSettleController', OrderSettleController);

    OrderSettleController.$inject = ['$scope', 'OrderMasterService', 'OrderDetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', "DeliveryService", "WarehouseService"];

    function OrderSettleController($scope, OrderMasterService, OrderDetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal, DeliveryService, WarehouseService) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.HQICExecutive = JSON.parse(localStorage.getItem('RolePerson'));

        {
            console.log("orderMasterController start loading OrderDetailsService");
            //User Tracking

            //$scope.DicountCalculate = function (data) {

            //    
            //    var cashamount = data.CashAmount ? data.CashAmount : 0;
            //    var Discount = data.DiscountAmount ? data.DiscountAmount : 0;
            //    var gr = data.TempCashAmount;
            //    if (cashamount > 0 && Discount <= gr && Discount != null) {
            //        //gr = gr - data.discounttc;
            //        //data.DiscountAmount = data.DiscountAmount;
            //        data.CashAmount = gr - Discount;

            //    }
            //    else
            //        data.CashAmount = data.TempCashAmount;
            //};

            $scope.DicountCalculate1 = function (data) {

                
                var recivedAmount = data.RecivedAmount ? data.RecivedAmount : 0;
                var Discount = data.DiscountAmount ? data.DiscountAmount : 0;
                var gr = data.TempCashAmount;
                if (recivedAmount > 0 && Discount <= gr && Discount != null) {
                    //gr = gr - data.discounttc;
                    //data.DiscountAmount = data.DiscountAmount;
                    data.RecivedAmount = gr - Discount;

                }
                else
                    data.RecivedAmount = data.TempCashAmount;
            };


            //End User Tracking
            $scope.enablebtn = function () {
                $scope.disable = "sccxz";
            }
            //new warehouse
            $scope.warehouse = [];
            $scope.getWarehosues = function () {
                
                WarehouseService.getwarehouse().then(function (results) {
                    
                    $scope.warehouse = results.data;
                    $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                    $scope.getWarehousebyId($scope.WarehouseId);
                    //$scope.getData($scope.pageno);
                }, function (error) {
                })
            };
            $scope.getWarehosues();
            $scope.pageno = 1; //initialize page no to 1
            $scope.total_count = 0;
            $scope.itemsPerPage = 40; //this could be a dynamic value from a drop down
            $scope.numPerPageOpt = [40, 50, 60];//dropdown options for no. of Items per page
            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;
            }
            $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown
            //DeliveryService.getdboys().then(function (results) {
            //    
            //    $scope.DBoys = results.data;
            //}, function (error) {
            //});
            $scope.getWarehousebyId = function (WarehouseId) {
                DeliveryService.getWarehousebyId($scope.WarehouseId).then(function (resultsdboy) {
                    $scope.DBoys = resultsdboy.data;
                }, function (error) {
                });
            };
            $scope.deliveryBoy = {};
            $scope.getData = function (pageno, Dboyno, startdate, enddate, OrderId, WarehouseId) {
                // This would fetch the data on page change.  //In practice this should be in a factory.
                $scope.customers = [];
                if (!Dboyno) Dboyno = "all";
                $scope.listMaster = [];
                if ($scope.deliveryBoy.Mobile) Dboyno = $scope.deliveryBoy.Mobile;
                var url = serviceBase + "api/OrderDispatchedMaster" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&DBoyNo=" + Dboyno + "&datefrom=" + startdate + "&dateto=" + enddate + "&OrderId=" + $scope.OrderId + "&WarehouseId=" + $scope.WarehouseId;
                $http.get(url).success(function (response) {
                    if (response.ordermaster.length == 0) {
                        alert("Not Found");
                    }
                    else {

                        $scope.listMaster = response.ordermaster;  //ajax request to fetch data into vm.data
                        $scope.listMasterold = angular.copy(response.ordermaster);
                        console.log("get all Order:");
                        console.log($scope.customers);
                        $scope.orders = $scope.customers;
                        $scope.total_count = response.total_count;
                        $scope.tempuser = response.ordermaster;
                    }

                });
            };

            $scope.getData($scope.pageno, "all", "", "", "");
            /// Search function with assignment id by raj
            $scope.getSearchdata = function (pageno, Dboyno, startdate, enddate, OrderId, DeliveryIssuanceIdOrderDeliveryMaster, WarehouseId) { // This would fetch the data on page change.  //In practice this should be in a factory.
              
                $scope.customers = [];
                if (!Dboyno) Dboyno = "all";

                if ($scope.deliveryBoy.Mobile) Dboyno = $scope.deliveryBoy.Mobile;

                var url = serviceBase + "api/OrderDispatchedMaster/klop" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&DBoyNo=" + Dboyno + "&datefrom=" + startdate + "&dateto=" + enddate + "&OrderId=" + $scope.OrderId + "&DeliveryIssuanceIdOrderDeliveryMaster=" + $scope.DeliveryIssuanceIdOrderDeliveryMaster + "&WarehouseId=" + $scope.WarehouseId;
               
                $http.get(url).success(function (response) {
                   
                    if (response.ordermaster.length == 0) {
                        alert("Not Found");
                    }
                    else {
                       
                        $scope.listMaster = response.ordermaster;  //ajax request to fetch data into vm.data
                       
                        $scope.listMasterold = angular.copy(response.ordermaster);
                        console.log("get all Order:");
                        console.log($scope.customers);
                        $scope.orders = $scope.customers;
                        $scope.total_count = response.total_count;
                        $scope.tempuser = response.ordermaster;
                    }
                });
            };

            $scope.checkAll = function () {

                $scope.disable = "sccxz";
                if ($scope.selectedAll) {
                    $scope.selectedAll = false;
                } else {
                    $scope.selectedAll = true;
                }
                angular.forEach($scope.listMaster, function (trade) {
                    trade.check = $scope.selectedAll;
                });

            };
            $scope.selectedsettled = function () {
                //
                $scope.assignedorders = [];
                $scope.selectedorders = [];
                for (var i = 0; i < $scope.listMaster.length; i++) {
                    if ($scope.listMaster[i].ShortAmount > 0 && ($scope.listMaster[i].ShortReason == "" || $scope.listMaster[i].ShortReason == undefined || $scope.listMaster[i].ShortReason == null || $scope.listMaster[i].ShortReason == "null")) {
                        alert('please select reason for short Amount');
                    }
                    else if ($scope.listMaster[i].check == false) {

                    }
                    else {
                        $scope.assignedorders.push($scope.listMaster[i]);
                    }
                }

                $scope.selectedorders = angular.copy($scope.assignedorders);
                var dataToPost = {
                    AssignedOrders: $scope.selectedorders
                };
                var url = serviceBase + 'api/OrderDispatchedMasterFinal/Multisettle';

                $http.post(url, dataToPost)
                    .success(function (data) {
                        //

                        if (data != null) {
                            alert(" Selected order settled successfully");
                            location.reload();
                        }
                        else {
                            alert("Selected order not settled");
                        }

                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })

            }

            $scope.filterstatus = function (data) {
                if (data.name === 'Show All') {
                    $scope.listMaster = $scope.listMasterold;
                } else {
                    $scope.listMaster = $filter('filter')($scope.listMasterold, { Status: data.name });
                }
            }



            $scope.hub = function (ord) {
                
                if (ord.DiscountAmount == 0) {
                    if (ord.ShortAmount > 0 && (ord.ShortReason == "" || ord.ShortReason == undefined || ord.ShortReason == null || ord.ShortReason == "null")) {
                    alert('please select reason for short Amount');
                }
                else {
                    if (confirm("Are you sure?")) {
                        var orddetails = ord.orderDetails;
                        var url = serviceBase + 'api/OrderDispatchedMasterFinal';
                        $http.post(url, ord)
                            .success(function (data) {
                                //
                                if (data != null) {
                                    //$scope.FdispatchedMasterID = data.FinalOrderDispatchedMasterId;
                                    //$scope.dispatchedDetailFinal(orddetails);
                                    $("#st" + data.OrderId).prop("disabled", true);
                                    $("#cst" + data.OrderId).prop("disabled", true);
                                    alert(" Selected order settled successfully");
                                    location.reload();

                                }
                                else {
                                    alert("Selected order not settled");
                                }

                            })
                            .error(function (data) {
                                console.log("Error Got Heere is ");
                                console.log(data);
                            })
                    }
                }

                }


                else {

                    console.log("Modal opened  ");
                    var modalInstance;
                    modalInstance = $modal.open(
                        {
                            templateUrl: "sattledReason.html",
                            controller: "sattledReasonController", resolve: { trade: function () { return ord } }
                        }), modalInstance.result.then(function (selectedItem) {
                            //$scope.currentPageStores.push(selectedItem);
                            console.log("modal close");
                            console.log(selectedItem);

                        },
                            function () {
                                console.log("Cancel Condintion");
                            })
                }
            }





            //$scope.Sattled = function (ord) {
            //    
            //    if (ord.ShortAmount > 0 && (ord.ShortReason == "" || ord.ShortReason == undefined || ord.ShortReason == null || ord.ShortReason == "null")) {
            //        alert('please select reason for short Amount');
            //    }
            //    else {
            //        if (confirm("Are you sure?")) {
            //            var orddetails = ord.orderDetails;
            //            var url = serviceBase + 'api/OrderDispatchedMasterFinal';
            //            $http.post(url, ord)
            //                .success(function (data) {
            //                    //
            //                    if (data != null) {
            //                        //$scope.FdispatchedMasterID = data.FinalOrderDispatchedMasterId;
            //                        //$scope.dispatchedDetailFinal(orddetails);
            //                        $("#st" + data.OrderId).prop("disabled", true);
            //                        $("#cst" + data.OrderId).prop("disabled", true);
            //                        alert(" Selected order settled successfully");

            //                    }
            //                    else {
            //                        alert("Selected order not settled");
            //                    }

            //                })
            //                .error(function (data) {
            //                    console.log("Error Got Heere is ");
            //                    console.log(data);
            //                })
            //        }
            //    }
            //}

           
            $(function () {
                
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A'
                })
  
                $('.input-group-addon').click(function () {
             $('input[name="daterange"]').trigger("select");
          

              });

           });

            $scope.Search = function (data) {
                
                $scope.dataforsearch = { datefrom: "", dateto: "" };
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                if (!$('#dat').val()) {
                    $scope.dataforsearch.datefrom = '';
                    $scope.dataforsearch.dateto = '';
                }
                else {
                    $scope.dataforsearch.datefrom = f.val();
                    $scope.dataforsearch.dateto = g.val();
                }
                if (data != undefined) {
                    $scope.deliveryBoy = JSON.parse(data);
                    if ($scope.deliveryBoy.Mobile) {
                        $scope.getSearchdata($scope.pageno, $scope.deliveryBoy.Mobile, $scope.dataforsearch.datefrom, $scope.dataforsearch.dateto, $scope.OrderId, $scope.DeliveryIssuanceIdOrderDeliveryMaster);
                    }
                }
                else if (data == undefined) {
                    $scope.getSearchdata($scope.pageno, "all", $scope.dataforsearch.datefrom, $scope.dataforsearch.dateto, $scope.OrderId, $scope.DeliveryIssuanceIdOrderDeliveryMaster);
                }
                else {
                    $scope.getSearchdata($scope.pageno, "all", $scope.dataforsearch.datefrom, $scope.dataforsearch.dateto, $scope.OrderId, $scope.DeliveryIssuanceIdOrderDeliveryMaster);
                }
            }

            $scope.show = true;
            $scope.order = false;

            $scope.showalldetails = function () {
                $scope.order = !$scope.order;
                $scope.show = !$scope.show;
                // $scope.callmethoddetails();
            };

            $scope.exportData = function (data) {
                
                $scope.OrderByDate = $scope.listMaster;
                console.log("export");
                if ($scope.OrderByDate.length <= 0) {
                    alert("No data available between two date ")
                }
                else {

                    $scope.NewExportData = [];
                    for (var i = 0; i < $scope.OrderByDate.length; i++) {

                        var tts = {
                            OrderId: '', CustomerName: '', WarehouseName: '', CreatedDate: '', DeliveryIssuanceIdOrderDeliveryMaster: '', PaymentAmount: '', CashAmount: '', CheckNo: '', CheckAmount: '', EpayLaterRefNo: '', EpayLaterAmount: '', OnlineRefNo: '', OnlineAmount: '', EmposRefNo: '', EmposAmount: '', DiscountAmount: '', deliveryCharge: '',
                            Settle: '', Status: '', Dboyno: ''
                        };
                        tts.WarehouseName = $scope.OrderByDate[i].WarehouseName;
                        tts.OrderId = $scope.OrderByDate[i].OrderId;
                        tts.AssignmentDate = $scope.OrderByDate[i].CreatedDate;               
                        tts.AssignmentNo = $scope.OrderByDate[i].DeliveryIssuanceIdOrderDeliveryMaster;
                        tts.OrderAmount = $scope.OrderByDate[i].GrossAmount;
                        tts.Discount = $scope.OrderByDate[i].DiscountAmount;
                       
                        tts.CashAmount = $scope.OrderByDate[i].CashAmount;
                        tts.ChequeNumber = $scope.OrderByDate[i].CheckNo;
                        tts.ChequeAmount = $scope.OrderByDate[i].CheckAmount;
                        tts.GullakAmount = $scope.OrderByDate[i].GullakAmount;
                        tts.OnlineAmount = $scope.OrderByDate[i].Online;
                        tts.EMposAmount = $scope.OrderByDate[i].Empos;
                        tts.EpayLaterAmount = $scope.OrderByDate[i].EpayLater;
                        tts.SettleAmount = $scope.OrderByDate[i].RecivedAmount;
                        tts.DeliveryBoyName	 = $scope.OrderByDate[i].DboyName;
                        tts.OrderedDate = $scope.OrderByDate[i].CreatedDate;
                        //tts.EpayLaterRefNo = $scope.OrderByDate[i].BasicPaymentDetails[2].TransRefNo;
                       
                        //tts.OnlineRefNo = $scope.OrderByDate[i].BasicPaymentDetails[4].TransRefNo;
                       
                        //tts.EmposRefNo = $scope.OrderByDate[i].BasicPaymentDetails[3].TransRefNo;
                        
                        
                        //tts.deliveryCharge = $scope.OrderByDate[i].deliveryCharge;
                       
                        //tts.Status = $scope.OrderByDate[i].Status;
                       
                        $scope.NewExportData.push(tts);
                    }
                    alasql.fn.myfmt = function (n) {
                        return Number(n).toFixed(2);
                    }
                }
                alasql('SELECT WarehouseName,OrderId,AssignmentDate,AssignmentNo,OrderAmount,Discount,CashAmount,ChequeNumber,ChequeAmount,OnlineAmount,EMposAmount,EpayLaterAmount,SettleAmount,DeliveryBoyName,OrderedDate INTO XLSX("OrderDetails.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);


            };
        }
    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .controller('sattledReasonController', sattledReasonController);

    sattledReasonController.$inject = ["$scope", '$http', 'WarehouseService', "$modalInstance", 'ngAuthSettings', "trade"];

    function sattledReasonController($scope, $http, WarehouseService, $modalInstance, ngAuthSettings, trade) {
        
       // $scope.trade = ord;
        var orderdetails = trade;
        var Reason = orderdetails;
     

        //by sudhir
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        //
       
            $scope.Sattled = function (Reason) {
                
                $scope.ODData = orderdetails;
                trade.Reason = Reason;


                
                if (orderdetails.ShortAmount > 0 && (orderdetails.ShortReason == "" || orderdetails.ShortReason == undefined || orderdetails.ShortReason == null || orderdetails.ShortReason == "null")) {
                    alert('please select reason for short Amount');
                }
                else {
                    if (confirm("Are you sure?")) {
                        var orddetails = orderdetails.orderDetails;
                        var url = serviceBase + 'api/OrderDispatchedMasterFinal';
                        $http.post(url, orderdetails)
                            .success(function (data) {
                                $modalInstance.close(data);
                                if (data != null) {
                                    
                                    //$scope.FdispatchedMasterID = data.FinalOrderDispatchedMasterId;
                                    //$scope.dispatchedDetailFinal(orddetails);
                                    $("#st" + data.OrderId).prop("disabled", true);
                                    $("#cst" + data.OrderId).prop("disabled", true);
                                    alert(" Selected order settled successfully");
                                    location.reload();

                                }
                                else {
                                    alert("Selected order not settled");
                                }

                            })
                            .error(function (data) {
                                console.log("Error Got Heere is ");
                                console.log(data);
                            })
                    }

                }
            }
        


    }
})();



