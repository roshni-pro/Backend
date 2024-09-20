'use strict'
app.controller('ComparisonCtrl', ['$scope', "$filter", "$http", "ngTableParams", function ($scope, $filter, $http, ngTableParams) {
    $scope.datrange = '';
    $scope.datefrom = '';
    $scope.dateto = '';
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A',
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
    }
    var Allbydate = [];
    var finallistoflist = [];
    $scope.filtype = "";
    $scope.type = "";

    var titleText = "";
    var legendText = "";
    
    $scope.selTime = [
        { value: 1, text: "Day" },
        { value: 2, text: "Month" },
        { value: 3, text: "Year" }
    ];
    $scope.selType = [
        { value: 1, text: "Total Order" },
        { value: 2, text: "Total sale" },
        { value: 3, text: "Active Retailer" }
    ];
    $scope.catType = [
        { value: 1, text: "Category" },
        { value: 2, text: "SubCatogary" },
        { value: 3, text: "SubSubCatogory" },
        { value: 4, text: "item" }
    ];
    $scope.selectType = [
        { value: 1, text: "Retailer" },
        { value: 2, text: "Hub" },
        { value: 3, text: "City" },
        { value: 4, text: "Executive" },
        { value: 5, text: "Cluster" }
    ];
    $scope.dataselect = [];
    $scope.examplemodel = [];
    $scope.exampledata = $scope.dataselect;
    $scope.examplesettings = {
        displayProp: 'Skcode', idProp: 'CustomerId',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };
    $scope.subexamplemodel = [];
    $scope.subexamplemodel = $scope.dataselect;
    $scope.subexamplesettings = {
        displayProp: 'WarehouseName', idProp: 'WarehouseId',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };
    $scope.ssubexamplemodel = [];
    $scope.ssubexamplemodel = $scope.dataselect;
    $scope.ssubexamplesettings = {
        displayProp: 'CityName', idProp: 'Cityid',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };
    $scope.Exeexamplemodel = [];
    $scope.Exeexamplemodel = $scope.dataselect;
    $scope.Exeexamplesettings = {
        displayProp: 'DisplayName', idProp: 'PeopleID',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };
    $scope.clusterModel = [];
    $scope.clusterModel = $scope.dataselect;
    $scope.clusterSetting = {
        displayProp: 'ClusterName', idProp: 'ClusterId',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };


    //(by neha : 11/09/2019 -date range )
    //$(function () {
    //    $('input[name="daterange"]').daterangepicker({
    //        timePicker: true,

    //        timePickerIncrement: 5,
    //        timePicker12Hour: true,
    //        format: 'DD/MM/YYYY h:mm A'
    //    }, function (start, end, label) {
    //        console.log("A new date selection was made: " + start.format('YYYY/MM/DD') + ' to ' + end.format('YYYY/MM/DD'));
    //    });

    //    $('.input-group-addon').click(function () {
    //        $('input[name="daterange"]').trigger("select");
    //        document.getElementsByClassName("daterangepicker")[0].style.display = "block";

    //    });
    //    //$('input[name="date"]').on('apply.daterangepicker', function (ev, picker) {
    //    //    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    //    //});
    //    //$('input[name="date"]').on('cancel.daterangepicker', function (ev, picker) {
    //    //    $(this).val('');
    //    //});

    //});







    $scope.selectedtypeChanged = function (data) {
        $scope.dataselect = [];
        var url = serviceBase + "api/Report/select?value=" + data.type;
        $http.get(url)
        .success(function (data) {
            if (data.length == 0) {
                alert("Not Found");
            }
            $scope.dataselect = data;
            console.log(data);
        });
    }

    $scope.selectedItemChanged = function (data) {
        $scope.dataselect1 = [];
        var url = serviceBase + "api/Report/Catogory?value=" + data.value;
        $http.get(url)
        .success(function (data) {
            if (data.length == 0) {
                alert("Not Found");
            }
            $scope.dataselect1 = data;
            console.log(data);
        });
    }

    $scope.zones = [];
    $scope.GetZones = function () {
        var url = serviceBase + 'api/inventory/GetZone';
        $http.get(url)
            .success(function (response) {
                $scope.zones = response;
            });
    };
    $scope.GetZones();

    $scope.regions = [];
    $scope.GetRegions = function (zone) {
        var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
        $http.get(url)
            .success(function (response) {
                $scope.regions = response;
            });
    };

    $scope.warehouses = [];
    $scope.GetWarehouses = function (warehouse) {
        var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        $http.get(url)
            .success(function (response) {
                $scope.warehouses = response;
            });
    };

    //$scope.clusters = [];
    //$scope.GetClusters = function (cluster) {
    //    var url = serviceBase + 'api/inventory/GetCluster?warehouseid=' + cluster;
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.clusters = response;
    //        });
    //};




    $scope.genereteReport = function (data1, filtype) {
        
        $scope.filtype = filtype;
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

        console.log("here daterange");
        console.log($scope.dataforsearch.datefrom);
        console.log($scope.dataforsearch.dateto);

        var ids = [];
        if (data1.type == 1) {
            _.each($scope.examplemodel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        } else if (data1.type == 2) {
            _.each($scope.subexamplemodel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        } else if (data1.type == 3) {
            _.each($scope.ssubexamplemodel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        } else if (data1.type == 4) {
            _.each($scope.Exeexamplemodel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        } else if (data1.type == 5) {
            _.each($scope.clusterModel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        }

        var url = "";
        if ($scope.filtype === "day") {
            $scope.frmt = "DD MMM";
            $scope.intervalType = "day";
            url = serviceBase + "api/Comparison/day?datefrom=" + $scope.dataforsearch.datefrom + "&dateto=" + $scope.dataforsearch.dateto + "&type=" + data1.type + "&ids=" + ids + "&value=" + data1.value + "&itemId=" + data1.id;
        }
        else if ($scope.filtype === "month") {
            $scope.frmt = "MMM YY";
            $scope.intervalType = "month";
            url = serviceBase + "api/Comparison/month?datefrom=" + $scope.dataforsearch.datefrom + "&dateto=" + $scope.dataforsearch.dateto + "&type=" + data1.type + "&ids=" + ids + "&value=" + data1.value + "&itemId=" + data1.id;
        }
        else if ($scope.filtype === "year") {
            $scope.frmt = "YYYY";
            $scope.intervalType = "year";
            url = serviceBase + "api/Comparison/year?datefrom=" + $scope.dataforsearch.datefrom + "&dateto=" + $scope.dataforsearch.dateto + "&type=" + data1.type + "&ids=" + ids + "&value=" + data1.value + "&itemId=" + data1.id;
        }
        
        $http.get(url)
        .success(function (data) {
            if (data.length == 0) {
                alert("Not Found");
                $scope.getChartOrder(null);
            }
            else {
                Allbydate = data;
            }
            console.log(data);
            finallistoflist = [];
            for (var i = 0; i < Allbydate.length; i++) {
                if ($scope.filtype === "") {
                    $scope.filtype = "day";
                }
                var fd = $scope.filterBydate(Allbydate[i].reports);
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
    
    $scope.filtertype = function (filterData) {
        var OrderSummary = [];
        for (var i = 0; i < filterData.length; i++) {
            if ($scope.type === "1") {
                var totalOrder = { x: new Date(filterData[i].dtt), y: filterData[i].totalOrder, name: filterData[i].name}
                OrderSummary.push(totalOrder);
                titleText = "Order's";
                legendText = "Total Order"
            }
            else if ($scope.type === "2") {
                var totalSale = { x: new Date(filterData[i].dtt), y: filterData[i].totalSale / 10000, name: filterData[i].name}
                OrderSummary.push(totalSale);
                titleText = "Sale's(in 10000)";
                legendText = "Total Sale"
            }
            else if ($scope.type === "3") {
                var totalSale = { x: new Date(filterData[i].dtt), y: filterData[i].activeRetailers, name: filterData[i].name }
                OrderSummary.push(totalSale);
                titleText = "Sale's(in 10000)";
                legendText = "Total Sale"
            }
        }
        //$scope.getChartOrder(OrderSummary, legendText);
        return OrderSummary;
    }

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
                name:gd1[0].name,
                markerType: "circle",
                color: clor,//"#F08080",
                dataPoints: gd1
            }
            return orsum;
        } else {
            return null;
        }
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
		        title: "Dates",
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
    }

    $scope.filterBydate = function (rep) {
        var filterData = [];
        angular.forEach(rep, function (value, key) {
            var obj = {};
            obj.totalOrder = value.totalOrder;
            obj.activeRetailers = value.activeRetailers;
            obj.totalSale = value.TotalAmount;
            obj.dtt = value.createdDate;
            obj.id = value.id;
            obj.name = value.name;
            filterData.push(obj);
        });
        if ($scope.type == "") {
            $scope.type = "1";
        };
        var fittypedata = $scope.filtertype(filterData);
        return fittypedata;
    }

    $scope.changeGraph = function (type) {
        $scope.type = type;
        finallistoflist = [];
        for (var i = 0; i < Allbydate.length; i++) {
            var fd = $scope.filterBydate(Allbydate[i].reports);
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
    }    
}]);