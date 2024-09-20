'use strict'
app.controller('unitEcoReportCtlr', ['$scope', '$filter', '$http', 'ngTableParams', 'WarehouseService', function ($scope, $filter, $http, ngTableParams, WarehouseService) {
    $scope.datrange = '';
    $scope.datefrom = '';
    $scope.dateto = '';
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker24Hour: true,
            format: 'YYYY-MM-DD h:mm A'
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
        var date = yyyy + '-' + mm + '-' + dd;
        return date;
    };

    WarehouseService.getwarehouse().then(function (results) {
        $scope.warehouses = results.data;
    }, function (error) {
    });

    var Allbydate = [];
    var finallistoflist = [];
    $scope.type = "";

    var titleText = "";
    var legendText = "";
    
    $scope.selType = [
       { value: 1, text: "Expense as % Sales" },
       { value: 2, text: "Expense per Order" }
    ];
    
    $scope.label1type = [
        { name: "Sales Exp" }, { name: "Logistic Exp" }, { name: "Operation Exp" }
    ]    
    $scope.examplemodel = [];    
    $scope.examplesettings = {
        displayProp: 'name', idProp: 'name',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    //$scope.label2type = [
    //    { name: "People" }, { name: "Marketing  & promo" }, { name: "Del Logistics" }, { name: "Pur logisticsp" }, { name: "Rent" }, { name: "Other exp" }
    //]
    //$scope.subexamplemodel = [];
    //$scope.subexamplesettings = {
    //    displayProp: 'name', idProp: 'name',
    //    scrollableHeight: '300px',
    //    scrollableWidth: '450px',
    //    enableSearch: true,
    //    scrollable: true
    //};




    $scope.genereteReport = function (data1) {
        
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

        var lab1 = [];
        _.each($scope.examplemodel, function (o2) {
            var Row = o2.id;
            lab1.push(Row);
        })
        var lab2 = [];
        _.each($scope.subexamplemodel, function (o2) {
            var Row = o2.id;
            lab2.push(Row);
        })
        $scope.frmt = "MMM YY";
        $scope.intervalType = "month";
        $scope.tableData = [];
        var url = serviceBase + "api/uniteconomicreport?datefrom=" + $scope.dataforsearch.datefrom + "&dateto=" + $scope.dataforsearch.dateto + "&Warehouseid=" + data1.WarehouseId + "&lab1="+ lab1;
        $http.get(url)
            .success(function (data) {
                
            if (data.length == 0) {
                alert("data doesn't exist...");
                $scope.getChartOrder(null);
            }
            else {
                Allbydate = data;
                //angular.forEach(data, function (value, key) {
                //    var trow = {};
                //    for (var i = 0; i < value.reportts.length; i++) {
                //        trow.month = value[i].createdDate;
                //        trow.totalExp = value[0].createdDate;

                //    }
                //});
            }
            console.log(data);
            finallistoflist = [];
            for (var i = 0; i < Allbydate.length; i++) {
                $scope.type = "1";
                var fd = $scope.filterBydate(Allbydate[i].reportts);
                if (fd.length > 0) {
                    finallistoflist.push(fd);
                }
            }
            var graphdata = [];
            for (var j = 0; j < finallistoflist.length; j++) {
                var gd = $scope.chartdata(finallistoflist[j], j);
                if (gd != null) {
                    graphdata.push(gd);
                }
            }
            $scope.getChartOrder(graphdata);
        });
    }
    
    $scope.getChartOrder = function (graphdata) {
        
        var chart = new CanvasJS.Chart("chartContainer",
		{
		    title: {
		        text: titleText,
		        fontSize: 30
		    },
		    animationEnabled: true,
		    axisX: {
		        intervalType: $scope.intervalType,
		        interval: 1,
		        gridColor: "Silver",
		        tickColor: "silver",
		        valueFormatString: $scope.frmt,
		        title: "Month",
		        labelAngle: 60,
		        titleFontSize: 25,
		        labelFontSize: 20,
		        abelAutoFit: true
		    },
		    toolTip: {
		        shared: true
		    },
		    theme: "theme1",
		    axisY: {
		        gridColor: "Silver",
		        tickColor: "silver",
		        title: legendText,
		        titleFontSize: 25,
		        labelFontSize: 20
		    },
		    legend: {
		        verticalAlign: "center",
		        horizontalAlign: "right",
		        fontSize: 25
		    },
		    data: graphdata,
		    legend: {
		        cursor: "pointer",
		        itemclick: function (e) {
		            if (typeof (e.dataSeries.visible) === "undefined" || e.dataSeries.visible) {
		                e.dataSeries.visible = false;
		            }
		            else {
		                e.dataSeries.visible = true;
		            }
		            chart.render();
		        }
		    }
		});
        chart.render();
    };

    $scope.filterBydate = function (rep) {
        
        var filterData = [];
        angular.forEach(rep, function (value, key) {
            var obj = {};
            obj.totalOrder = value.totalOrder;
            obj.totalSale = value.totalSale;
            obj.totalExpence = value.totalExpence;
            obj.name = value.name;
            obj.dtt = value.createdDate;
            filterData.push(obj);
        });        
        var fittypedata = $scope.filtertype(filterData);
        return fittypedata;
    };

    $scope.chartdata = function (gd1, j) {
        
        if (gd1.length > 0) {
            var clor = ""
            if (j == 0) { clor = "grey" }
            else if (j == 1) { clor = "forestgreen" }
            else if (j == 2) { clor = "red" }
            else if (j == 3) { clor = "pink" }
            else if (j == 4) { clor = "yellow" }
            else if (j == 5) { clor = "blue" }
            else if (j == 6) { clor = "brown" }
            else { clor = "black" }
            var orsum = {
                type: "line",
                showInLegend: true,
                lineThickness: 2,
                name: gd1[0].name,
                markerType: "circle",
                color: clor,//"#F08080",
                dataPoints: gd1
            }
            return orsum;
        } else {
            return null;
        }


    };
    
    $scope.filtertype = function (filterData) {
        
        var OrderSummary = [];
        for (var i = 0; i < filterData.length; i++) {
            if ($scope.type === "1") {
                var y = 0;
                if (filterData[i].totalExpence != 0)
                    y = (filterData[i].totalExpence / filterData[i].totalSale) * 100;
                var totalOrder = { x: new Date(filterData[i].dtt), y: y, name: filterData[i].name }
                OrderSummary.push(totalOrder);
                legendText = "Values in %";
                titleText = "Expense as % Sales";
            }
            else if ($scope.type === "2") {
                var y = 0;
                if (filterData[i].totalExpence != 0)
                    y =filterData[i].totalExpence / filterData[i].totalOrder;
                var totalSale = { x: new Date(filterData[i].dtt), y: y, name: filterData[i].name }
                OrderSummary.push(totalSale);
                legendText = "Values";
                titleText = "Expense per Order"
            }
        }
        return OrderSummary;
    };

    $scope.changeGraph = function (type) {
        
        $scope.type = type;
        finallistoflist = [];
        for (var i = 0; i < Allbydate.length; i++) {
            var fd = $scope.filterBydate(Allbydate[i].reportts);
            if (fd.length > 0) {
                finallistoflist.push(fd);
            }
        }
        var graphdata = [];
        for (var j = 0; j < finallistoflist.length; j++) {
            var gd = $scope.chartdata(finallistoflist[j], j);
            if (gd != null) {
                graphdata.push(gd);
            }

        }
        $scope.getChartOrder(graphdata);
    };
}]);