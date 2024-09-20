
(function () {
    'use strict';
    angular.module('app').controller('OrderProcessReportController', OrderProcessReportController);

    OrderProcessReportController.$inject = ['$scope', "$http", "ngTableParams", "WarehouseService"];

    function OrderProcessReportController($scope, $http, ngTableParams, WarehouseService) {


        $scope.selectType = [
            { value: "Issued", text: "Issued" },
            { value: "Shipped", text: "Shipped" },
            { value: "sattled", text: "sattled" },
            { value: "Pending", text: "Pending" },
            { value: "Delivered", text: "Delivered" }
        ];
        $scope.warehouse = [];

        //WarehouseService.getActivewarehouse().then(function (results)
        //{           
        //    $scope.warehouse = results.data;
        //}, function (error) {
        //});
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                });

        };
        $scope.wrshse();





         $scope.examplemodel = [];
    $scope.exampledata = $scope.warehouse;
    $scope.examplesettings = {
        displayProp: 'label', idProp: 'value',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
        };

        $scope.dbReport = [];
        $scope.Search = function (data)
        {           
            $scope.dbReport = [];

            let whids = $scope.examplemodel.map(a => a.id);
            if (whids.length >0)
            {
                var url = serviceBase + "api/OrederProcessReport?WarehouseId=" + whids;// + '&datefrom=' + start + '&dateto=' + end;
                $http.get(url).success(function (results) {

                    $scope.dbReport = results;
                });
            }
            else {
                alert("Please select Warehouse");                
           }
        };

        $scope.Export = function (data)
        {


            alasql('SELECT OrderId,Skcode, AssignmentId,DboyName,Status,invoice_no,Dispatechdate,OrderedDate,WarehouseName,UpdatedDate,Deliverydate,GrossAmount,deliveryCharge,DeliveredDate,BillDiscountAmount,WalletAmount,DiscountAmount,OrderTypestr,IsKPP,DeliveryCanceledComments,ReAttemptCount,CustomerType,StoreName,PaymentFrom as PaymentType INTO XLSX("OrderProcessReport.xlsx",{headers:true}) FROM ?', [data]);
        };

        $scope.ExportV = function (data) {
            alasql('SELECT OrderId,Skcode, AssignmentId,DboyName,Status,invoice_no,Dispatechdate,WarehouseName,UpdatedDate,Deliverydate,OrderedDate,GrossAmount,deliveryCharge,DeliveredDate,BillDiscountAmount,WalletAmount,DiscountAmount,OrderTypestr,IsKPP,DeliveryCanceledComments,ReAttemptCount,CustomerType,StoreName,PaymentFrom as PaymentType INTO XLSX("OrderProcessReport.xlsx",{headers:true}) FROM ?', [data]);
        };
        $scope.OrderDeliveryCanceled = function (data) {
            
            alasql('SELECT OrderId,Skcode, AssignmentId,DboyName,invoice_no,Dispatechdate,Status,WarehouseName,UpdatedDate,Deliverydate,ReDispatchCount,GrossAmount,deliveryCharge,BillDiscountAmount,WalletAmount,DiscountAmount,OrderedDate,DeliveredDate,DeliveryCanceledDate,Redispatchdate,OrderTypestr,IsKPP,ReAttemptCount,CustomerType,StoreName,PaymentFrom as PaymentType INTO XLSX("OrderProcessReport.xlsx",{headers:true}) FROM ?', [data]);
        };

        $scope.ExportAll = function ()
        {
            alasql('SELECT OrderId,Skcode, AssignmentId,DboyName,invoice_no,Dispatechdate,Status,OrderedDate,WarehouseName,UpdatedDate,Deliverydate,GrossAmount,deliveryCharge,DeliveredDate,BillDiscountAmount,WalletAmount,DiscountAmount,OrderTypestr,IsKPP,DeliveryCanceledComments,comments,ReDispatchCount,CustomerType,ReAttemptCount,StoreName,PaymentFrom as PaymentType INTO XLSX("OrderProcessReport.xlsx",{headers:true}) FROM ?', [$scope.dbReport.ExportAll]);
        };

        $scope.examplemodell = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesetting = {
            displayProp: 'label', idProp: 'value',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };

        $scope.qrReport = [];

        $scope.SearchQReports = function (data, Waithrs) {
            $scope.qrReport = [];
            debugger;
            console.log($scope.examplemodell)
            let whids = [];

            $scope.examplemodell.map(a => a.id);
            $scope.examplemodell.forEach(element => {
                whids.push(element.id)
            });

            if (whids.length > 0) {
                var url = serviceBase + "api/OrederProcessReport/GetOrderQueueDashboard?warehouseid=" + whids + "&hours=" + Waithrs;
                $http.post(url).success(function (results) {
                    //debugger;

                    $scope.qrReport = results;
                    $scope.qrReport.forEach(el => {
                        if (el.Status == 'Pending') {
                            $scope.pendingcount = el.OrderCount
                            $scope.pendingAmount = el.TotalAmount
                            $scope.pendingwaithr = el.waithr
                            $scope.pendingStatus = el.Status
                        }

                        if (el.Status == 'ReadyToPick') {
                            $scope.RTPcount = el.OrderCount
                            $scope.RTPAmount = el.TotalAmount
                            $scope.RTPwaithr = el.waithr
                            $scope.RTPstatus = el.Status
                        }

                        if (el.Status == 'Ready to Dispatch') {
                            $scope.RTDcount = el.OrderCount
                            $scope.RTDAmount = el.TotalAmount
                            $scope.RTDwaithr = el.waithr
                            $scope.RTDstatus = el.Status
                        }

                        if (el.Status == 'Issued') {
                            $scope.Issuedcount = el.OrderCount
                            $scope.IssuedAmount = el.TotalAmount
                            $scope.Issuedwaithr = el.waithr
                            $scope.Issuedstatus = el.Status
                        }

                        if (el.Status == 'Shipped') {
                            $scope.Shippedcount = el.OrderCount
                            $scope.ShippedAmount = el.TotalAmount
                            $scope.Shippedwaithr = el.waithr
                            $scope.Shippedstatus = el.Status
                        }

                        if (el.Status == 'Delivered') {
                            $scope.Deliveredcount = el.OrderCount
                            $scope.DeliveredAmount = el.TotalAmount
                            $scope.Deliveredwaithr = el.waithr
                            $scope.Deliveredstatus = el.Status
                        }

                        if (el.Status == 'Delivery Redispatch') {
                            $scope.DeliveredRedispatchcount = el.OrderCount
                            $scope.DeliveredRedispatchAmount = el.TotalAmount
                            $scope.DeliveredRedispatchwaithr = el.waithr
                            $scope.DeliveredRedispatchstatus = el.Status
                        }

                        if (el.Status == 'ReAttempt') {
                            $scope.ReAttemptcount = el.OrderCount
                            $scope.ReAttemptAmount = el.TotalAmount
                            $scope.ReAttemptwaithr = el.waithr
                            $scope.ReAttemptstatus = el.Status
                        }

                        if (el.Status == 'Delivery Canceled') {
                            $scope.DeliveryCanceledcount = el.OrderCount
                            $scope.DeliveryCanceledAmount = el.TotalAmount
                            $scope.DeliveryCanceledwaithr = el.waithr
                            $scope.DeliveryCanceledstatus = el.Status
                        }

                    })
                    console.log("$scope.qrReport", $scope.qrReport);
                });
            }
            else {
                alert("Please select Warehouse");
            }
        };

        $scope.ExportQRreport = function (hrs, status, Waithrs) {
            debugger;
            console.log($scope.examplemodell)
            let whids = [];

            $scope.examplemodell.map(a => a.id);
            $scope.examplemodell.forEach(element => {
                whids.push(element.id)
            });

            if (whids.length > 0) {
                let ty = Waithrs == undefined ? hrs : Waithrs
                var url = serviceBase + "api/OrederProcessReport/GetOrderQueueDashboardDeatils?warehouseid=" + whids + "&status=" + status + "&hours=" + ty; //?warehouseid=1,169,213&status="Pending"&hours=0
                $http.post(url).success(function (results) {
                    alasql('SELECT OrderId,Skcode, WarehouseName,OrderStatus,CustomerType,OrderDate,Deliverydate,UpdatedDate,CurrentStatusDate,Wait_Hr,ReadyToPickCount INTO XLSX("OrderQueueReport.xlsx",{headers:true}) FROM ?', [results]);

                })
            }
        }

        $scope.Statuss ="";
        $scope.hr = 0;
        $scope.ExportAlls = function () {
            debugger;

            let whids = [];
            $scope.warehouse.map(a => a.id);
            $scope.warehouse.forEach(element => {
                whids.push(element.value)
            });
            if (whids.length > 0) {
                var url = serviceBase + "api/OrederProcessReport/GetOrderQueueDashboardDeatils?warehouseid=" + whids + "&status=" + $scope.Statuss + "&hours=" + $scope.hr;
                $http.post(url).success(function (result) {
                    console.log(result)
                    alasql('SELECT OrderId,Skcode, WarehouseName,OrderStatus,CustomerType,OrderDate,Deliverydate,UpdatedDate,CurrentStatusDate,waithr,ReadyToPickCount INTO XLSX("ExportAllOrderQueueReport.xlsx",{headers:true}) FROM ?', [result]);


                })
            }
        }

    }
})();




