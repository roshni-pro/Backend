'use strict'
app.controller('ReportController', ['$scope', "$filter", "$http", "ngTableParams", "WarehouseService", function ($scope, $filter, $http, ngTableParams, WarehouseService) {
    var OrderSummary = [];
    $scope.warehouse = [];
    $scope.WarehouseId = 1;
    //WarehouseService.getwarehouse().then(function (results) {        
    //    $scope.warehouse = results.data;
    //    $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
    //    $scope.genereteReport();
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



    $scope.selectedwarehouse = function () {
        OrderSummary = [];
        $scope.genereteReport();
    };

    $scope.getChartOrder = function (data) {

        var chart = new CanvasJS.Chart("chartContainer",
           {
               backgroundColor: "#b6bdc6",
               title: {
                   text: "Live Hub KPI's",
                   fontSize: 40,
               },
               dataPointMaxWidth: 120,
               animationEnabled: true,
               axisX: {
                   labelAngle: -30,
                   titleFontSize: 25,
                   labelFontSize: 25,
                   labelFontWeight: "bold",
                   labelFontColor: "#535653",
                   gridColor: "Silver",
                   tickColor: "silver",
                   abelAutoFit: true
               },
               axisY: {
                   gridColor: "Silver",
                   tickColor: "silver",
                   titleFontSize: 25,
                   labelFontSize: 25,
                   labelFontWeight: "bold",
                   labelFontColor: "#535653",
                   titleFontWeight: "bold",
                   titleFontColor: "#535653",
                   title: "Order/Sale(in 10000)",
               },
               legend: {
                   verticalAlign: "bottom",
                   horizontalAlign: "center"
               },
               theme: "theme3",
               data: [
               {
                   type: "column",
                   yValueFormatString: "####.###",
                   indexLabel: "{y}",
                   indexLabelFontColor: "black",
                   showInLegend: true,
                   legendMarkerColor: "grey",
                   legendText: "Today",
                   dataPoints: data
               }
               ]
           });
        chart.render();
    };
 
    $scope.filterall = function () {
  
        $scope.customerdata = [];
        var url = serviceBase + 'api/CustomersReport/all';
        $http.get(url)
        .success(function (data) {
            if (data.length == 0) {
                alert("Not Found");
            }
            $scope.customerdata = data;
            $scope.total = $scope.customerdata.length;
            var a = $scope.customerdata.length;

            $(function () {
                var chart = new CanvasJS.Chart("chartContainerall",
                {
                    title: {
                        text: "Total registered customer",
                        fontFamily: "arial black"
                    },
                    animationEnabled: true,
                    legend: {
                        verticalAlign: "bottom",
                        horizontalAlign: "center"
                    },
                    credits: {
                        enabled: false
                    },
                    theme: "theme1",
                    data: [
                    {
                        type: "pie",
                        indexLabelFontFamily: "Garamond",
                        indexLabelFontSize: 20,
                        indexLabelFontWeight: "bold",
                        startAngle: 0,
                        indexLabelFontColor: "MistyRose",
                        indexLabelLineColor: "darkgrey",
                        indexLabelPlacement: "inside",
                        toolTipContent: "{name}: {y}",
                        showInLegend: true,
                        // indexLabel: "#percent%",
                        indexLabel: "{y}",
                        dataPoints: [
                            { y: a, name: "Total registration", legendMarkerType: "circle" }
                        ]
                    }
                    ]
                });
                chart.render();
            });
        });
    };

    $scope.filteractive = function () {
        
        $scope.customerdata = [];
        var url = serviceBase + 'api/CustomersReport/all';
        $http.get(url)
        .success(function (data) {
            if (data.length == 0) {
                alert("Not Found");
            }
            $scope.customerdata = data;
            var a = $scope.customerdata.length;


            var count = 0;
            for (var i = 0; i < data.length; i++) {
                if (data[i].Active == true)
                    count++;
            }

            console.log(count);

            var counti = 0;
            for (var i = 0; i < data.length; i++) {
                if (data[i].Active == false)
                    counti++;
            }

            console.log(counti);



            $(function () {
                var chart = new CanvasJS.Chart("chartContaineractiveinactive",
                {
                    title: {
                        text: "active & inactive customers",
                        fontFamily: "arial black"
                    },
                    animationEnabled: true,
                    legend: {
                        verticalAlign: "bottom",
                        horizontalAlign: "center"
                    },
                    credits: {
                        enabled: false
                    },
                    theme: "theme1",
                    data: [
                    {
                        type: "pie",
                        indexLabelFontFamily: "Garamond",
                        indexLabelFontSize: 20,
                        indexLabelFontWeight: "bold",
                        startAngle: 0,
                        indexLabelFontColor: "MistyRose",
                        indexLabelLineColor: "darkgrey",
                        indexLabelPlacement: "inside",
                        toolTipContent: "{name}: {y}",
                        showInLegend: true,
                        // indexLabel: "#percent%",
                        indexLabel: "{y}",
                        dataPoints: [
                    { y: count, name: "Active customers", legendMarkerType: "triangle" },
				{ y: counti, name: "Inactive customers", legendMarkerType: "square" }

                        ]
                    }
                    ]
                });
                chart.render();
            });
        });
    };

    $scope.totalOrder = 0;
    $scope.totalSale = 0;
    $scope.pendingOrder = 0;
    $scope.pendinSale = 0;
    $scope.pendingOrder2 = 0;
    $scope.pendinSale2 = 0;
    $scope.totalDelivered = 0;
    $scope.DeliveredSale = 0;
    $scope.cancelOrder = 0;

    $scope.genereteReport = function () {
        var url = serviceBase + "api/Report/first?type=report1&Warehouseid=" + $scope.WarehouseId;

    
        //var url = serviceBase + "api/Report/first?type=report1&WarehouseId=" + $scope.WarehouseId;
        $http.get(url)
        .success(function (data) {
            if (data.length == 0) {
                alert("Not Found");
            }
            else {
                $scope.totalOrder = data.totalOrder;
                $scope.totalSale = data.totalSale;
                $scope.pendingOrder = data.pendingOrder;

                $scope.pendinSale = data.PendingSale;
                $scope.pendingOrder2 = data.pendingOrder_2;
                $scope.pendinSale2 = data.PendingSale2;
                $scope.NotDeliverOrder2 = data.NotDeliveredOrder_2;
                $scope.NotDeliverSale2 = data.NotDeliveredSale_2;
                $scope.NotDelivered = data.NotDelivered;
                $scope.NotDeliveredSale = data.NotDeliveredSale;
                $scope.totalDelivered = data.totalDelivered;
                $scope.DeliveredSale = data.deliveredSale;
                $scope.cancelOrder = data.cancelOrder;

                var totalOrder = { y: data.totalOrder, label: "Today Order's" }
                var totalSale = { y: data.totalSale / 10000, label: "Today Sale's" }
                var pendingOrder = { y: data.pendingOrder, label: "All Pending Order's" }
                var pendinSale = { y: data.PendingSale / 10000, label: "All Pending Sale's" }
                var pendingOrder2 = { y: data.pendingOrder_2, label: "Pending > 2 Day" }
                var pendinSale2 = { y: data.PendingSale2 / 10000, label: "Pending Sale > 2 Day" }
                var NotDelivered = { y: data.notDelivered, label: "Not Deliver >2 Day Order's" }
                var NotDeliveredSale = { y: data.notDeliveredSale / 10000, label: "Not Deliver >2 Day Sale's" }
                var totalDelivered = { y: data.totalDelivered, label: "Delivered" }
                var DeliveredSale = { y: data.deliveredSale / 10000, label: "Delivered Sale" }
                var cancelOrder = { y: data.cancelOrder, label: "Canceled Order" }
                OrderSummary.push(totalOrder);
                OrderSummary.push(totalSale);
                OrderSummary.push(NotDelivered);
                OrderSummary.push(NotDeliveredSale);
                OrderSummary.push(pendingOrder);
                OrderSummary.push(pendinSale);
                OrderSummary.push(pendingOrder2);
                OrderSummary.push(pendinSale2);
                OrderSummary.push(totalDelivered);
                OrderSummary.push(DeliveredSale);
                OrderSummary.push(cancelOrder);
            }
            console.log(OrderSummary);
            $scope.getChartOrder(OrderSummary);
        });
    };

}]);