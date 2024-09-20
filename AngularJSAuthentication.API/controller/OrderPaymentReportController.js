
(function () {
    'use strict';

    angular
        .module('app')
        .controller('OrderPaymentReportController', OrderPaymentReportController);

    OrderPaymentReportController.$inject = ['$scope', "$filter", "$http", "ngTableParams"];

    function OrderPaymentReportController($scope, $filter, $http, ngTableParams) {

        $scope.startdate = '';
        $scope.datrange = '';
        $scope.datefrom = '';
        $scope.dateto = '';
        $scope.dby = true;
        var bydate = [];
        //document.getElementById("dby").hidden = true;
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                //locale: {
                format: 'MM/DD/YYYY h:mm A'
                //}
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });

        $scope.getFormatedDate = function (date) {
            console.log("here get for")
            date = new Date(date);
            var dd = date.getDate();
            var mm = date.getMonth() + 1; //January is 0!

            var yyyy = date.getFullYear();
            if (dd < 10) {
                dd = '0' + dd;
            }
            if (mm < 10) {
                mm = '0' + mm;
            }
            var date1 = yyyy + '-' + mm + '-' + dd;
            return date;
        }
        $scope.GrandTotal = 0;
        $scope.GrandTotal1 = 0
        $scope.Ordercount = 0;

        $scope.filter = function () {
            $scope.dby = true;
            $scope.filterdata = [];
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

            $scope.ordercount = 0;
            console.log("here daterange");
            console.log($scope.dataforsearch.datefrom);
            console.log($scope.dataforsearch.dateto);
            $scope.orders = [];



            $scope.orders = [];
            //   var url = serviceBase + 'api/OrderDispatchedDetailsFinal?id=' + 2;
            var url = serviceBase + 'api/OrderDispatchedDetailsFinal/GetReport?datefrom=' + $scope.dataforsearch.datefrom + "&dateto=" + $scope.dataforsearch.dateto;
            $http.get(url)
                .success(function (data) {


                    if (data.length == 0) {
                        alert("Not Found");

                    }

                    $scope.orders = data;
                    $scope.getChart();
                    console.log("purvi data here");
                    console.log($scope.orders);

                    for (var j = 0; j < $scope.orders.length; j++) {

                        $scope.Ordercount = $scope.orders[j].ordercount;


                        $scope.GrandTotal = $scope.GrandTotal + $scope.orders[j].priceTotaltotal;
                        console.log("purvi grand total data here");
                        console.log($scope.GrandTotal);
                    }
                });

        }//filter

        $scope.getChart = function () {

            bydate = [];

            _.map($scope.orders, function (obj5) {
                console.log("chart obj5 here");
                console.log(obj5);

                var ob = { y: obj5.priceTotaltotal, label: $scope.getFormatedDate(obj5.OrderDate) }
                console.log("chart ob here");
                console.log(ob);
                bydate.push(ob);
                console.log("chart here");
                console.log(bydate);
            })



            var chart = new CanvasJS.Chart("chartContainer",
                {
                    title: {
                        text: "Payment Details"
                    },
                    animationEnabled: true,
                    axisY: {
                        title: "Values(In Thousand)"
                    },
                    legend: {
                        verticalAlign: "bottom",
                        horizontalAlign: "center"
                    },
                    theme: "theme2",
                    data: [

                        {
                            type: "column",
                            showInLegend: true,
                            legendMarkerColor: "grey",
                            legendText: "Total Sum Per Day",
                            dataPoints: bydate
                        }
                    ]
                });

            chart.render();

        }

        $scope.Report = [];
        $scope.DeliveryBoy = function () {
            $scope.dby = false;
            $scope.filterdata = [];
            $scope.dataforsearch = { datefrom: "", dateto: "" };
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            $scope.dataforsearch.datefrom = f.val();
            $scope.dataforsearch.dateto = g.val();

            $scope.Report.push($scope.dataforsearch);

            $scope.ordercount = 0;
            console.log("here daterange");
            console.log($scope.Report);

            $scope.delOrders = [];
            var url = serviceBase + 'api/OrderDispatchedMaster?datefrom=' + $scope.dataforsearch.datefrom + "&dateto=" + $scope.dataforsearch.dateto + "&id=1";
            $http.get(url)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("no Record found between this date");
                    }
                    $scope.delOrders = data;
                    console.log(data);
                });
        }


        $scope.DeliveryBoyr = function (db) {
            var Dboy = db.DboyName;
            var url = serviceBase + 'api/OrderDispatchedMaster?datefrom=' + $scope.dataforsearch.datefrom + "&dateto=" + $scope.dataforsearch.dateto + "&DboyName=" + Dboy + "&id=1";
            $http.get(url)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("no Record found between this date");
                    }
                    $scope.DelReport = data;
                    console.log(data);
                    alasql.fn.myfmt = function (n) {
                        return Number(n).toFixed(2);
                    }
                    alasql('SELECT OrderDispatchedMasterId,DboyName,DboyMobileNo,Deliverydate,CustomerId,CustomerName,Customerphonenum,ShippingAddress,OrderDate,Status,TotalAmount,GrossAmount,TaxAmount,SalesPerson INTO XLSX("Delivery Report.xlsx",{headers:true}) FROM ?', [$scope.DelReport]);
                });
        }

    }
})();