'use strict'
app.controller('Comparison1Ctrl', ['$scope', "$filter", "$http", "ngTableParams", function ($scope, $filter, $http, ngTableParams) {
    $scope.datrange = '';
    $scope.datefrom = '';
    $scope.dateto = '';
    $scope.DataToExport = [];
    $scope.FileUrl = "";
    $scope.report = [];
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
        { value: 3, text: "Active Retailer" },
        { value: 4, text: "Active Brand" },
        { value: 5, text: "Online Sales" },
        { value: 6, text: "Kisan Kirana Sales" }

    ];
    $scope.selectType = [
        { value: 4, text: "Excecutive" },
        { value: 2, text: "Hub" },
        { value: 3, text: "City" },
        { value: 5, text: "Cluster" },
        { value: 6, text: "Store" },
        { value: 7, text: "Diy Data" }
    ];
    $scope.dataselect = [];
    $scope.examplemodel = [];
    $scope.exampledata = $scope.dataselect;
    $scope.examplesettings = {
        displayProp: 'DisplayName', idProp: 'PeopleID',
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
    $scope.clusterModel = [];
    $scope.clusterModel = $scope.dataselect;
    $scope.clusterSetting = {
        displayProp: 'ClusterName', idProp: 'ClusterId',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.storeModel = [];
    $scope.storeSetting = {
        displayProp: 'Name', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };




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
        if (data1.type == 4) {
            _.each($scope.examplemodel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        }
        else if (data1.type == 2 || data1.type == 7) {
            _.each($scope.subexamplemodel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        }
        else if (data1.type == 3) {
            _.each($scope.ssubexamplemodel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        }
        else if (data1.type == 5) {
            _.each($scope.clusterModel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
        }
        else if (data1.type == 6) {
            _.each($scope.storeModel, function (o2) {
                var Row = o2.id;
                ids.push(Row);
            })
      
        }

        var url = "";
        var seltype = 1;
        if ($scope.selType.value !== undefined)
            seltype = $("#seltype").val();

        $scope.frmt = "DD MMM";
        $scope.intervalType = "day";

        var requestParam = {
            datefrom: $scope.dataforsearch.datefrom,
            dateto: $scope.dataforsearch.dateto,
            type: data1.type,
            ids: ids,
            selType: seltype,
            dateFormat: filtype
        };

        url = serviceBase + "api/Comparison1/day";

        $http.post(url, requestParam)
            .success(function (data) {

                if (data.length == 0) {
                    alert("Not Found");
                    //$scope.getChartOrder(null);
                }
                else {
                    Allbydate = data;

                }
                //console.log(data);
                //finallistoflist = [];

                //for (var i = 0; i < Allbydate.length; i++) {
                //    finallistoflist.push(Allbydate[i].GraphData)
                //}

                //var temp = finallistoflist[0];
                //for (var i = 0; i < temp.length; i++) {
                //    for (var j = 0; j < temp[i].dataPoints.length; j++) {
                //        temp[i].dataPoints[j].x = new Date(temp[i].dataPoints[j].x);
                //    }
                //}

                //$scope.report = Allbydate[0].reports;
                //$scope.DataToExport = Allbydate[0].ConsolidatedList;
                $scope.FileUrl = Allbydate[0].FileUrl;
                console.log('File Url', $scope.FileUrl);
                download($scope.FileUrl, "HubCityReport.xlsx");
                //finallistoflist = temp;
                //$scope.dateMonthYear(filtype);
                //$scope.getChartOrder(finallistoflist, filtype);
            });
    }


    $scope.filtertype = function () {

        if ($scope.type === "1") {
            titleText = "Order's";
            legendText = "Total Order"

            for (var i = 0; i < finallistoflist.length; i++) {
                for (var j = 0; j < finallistoflist[i].dataPoints.length; j++) {
                    finallistoflist[i].dataPoints[j].y = finallistoflist[i].dataPoints[j].totalOrder;  //new Date(finallistoflist[i].dataPoints[j].x);
                }
            }

        }
        else if ($scope.type === "2") {
            titleText = "Sale's(in 10000)";
            legendText = "Total Sale"
            for (var i = 0; i < finallistoflist.length; i++) {
                for (var j = 0; j < finallistoflist[i].dataPoints.length; j++) {
                    finallistoflist[i].dataPoints[j].y = finallistoflist[i].dataPoints[j].TotalAmount;  //new Date(finallistoflist[i].dataPoints[j].x);
                }
            }
        }
        else if ($scope.type === "3") {
            titleText = "Retailer's";
            legendText = "Active Retailers"
            for (var i = 0; i < finallistoflist.length; i++) {
                for (var j = 0; j < finallistoflist[i].dataPoints.length; j++) {
                    finallistoflist[i].dataPoints[j].y = finallistoflist[i].dataPoints[j].activeRetailers;  //new Date(finallistoflist[i].dataPoints[j].x);
                }
            }
        }
        else if ($scope.type === "4") {
            titleText = "Brand's";
            legendText = "Active Brands"
            for (var i = 0; i < finallistoflist.length; i++) {
                for (var j = 0; j < finallistoflist[i].dataPoints.length; j++) {
                    finallistoflist[i].dataPoints[j].y = finallistoflist[i].dataPoints[j].activeBrands;  //new Date(finallistoflist[i].dataPoints[j].x);
                }
            }
        }
        else if ($scope.type === "5") {
            titleText = "Online";
            legendText = "Online Sale"
            for (var i = 0; i < finallistoflist.length; i++) {
                for (var j = 0; j < finallistoflist[i].dataPoints.length; j++) {
                    finallistoflist[i].dataPoints[j].y = finallistoflist[i].dataPoints[j].OnlineSales;  //new Date(finallistoflist[i].dataPoints[j].x);
                }
            }
        }
        else if ($scope.type === "6") {
            titleText = "Kisan Kirana";
            legendText = "Kisan Kirana Sale"
            for (var i = 0; i < finallistoflist.length; i++) {
                for (var j = 0; j < finallistoflist[i].dataPoints.length; j++) {
                    finallistoflist[i].dataPoints[j].y = finallistoflist[i].dataPoints[j].KisanKiranaSales;  //new Date(finallistoflist[i].dataPoints[j].x);
                }
            }
        }


        $scope.getChartOrder(finallistoflist);
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
                type: "spline",
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


    }

    $scope.getChartOrder = function (graphdata, xFormat) {

        var chart = new CanvasJS.Chart("chartContainer",
            {
                title: {
                    text: titleText,
                    fontSize: 30
                },
                animationEnabled: true,
                axisX: {
                    intervalType: xFormat,
                    interval: xFormat != "day" ? 1 : 0,
                    //gridColor: "Silver",
                    //tickColor: "silver",
                    //valueFormatString: $scope.frmt,
                    title: "Dates",
                    //labelAngle: 60,
                    //titleFontSize: 25,
                    //labelFontSize: 20,
                    abelAutoFit: true
                    //valueFormatString: xFormat //"DD MMM,YY"
                },
                toolTip: {
                    shared: true
                },
                theme: "theme1",
                axisY: {
                    //gridColor: "Silver",
                    //tickColor: "silver",
                    //title: legendText,
                    //titleFontSize: 25,
                    //labelFontSize: 20
                    title: legendText,

                },
                //legend: {
                //    //verticalAlign: "center",
                //    //horizontalAlign: "right",
                //    //fontSize: 25
                //    cursor: "pointer",
                //    fontSize: 16,
                //},
                data: graphdata,
                legend: {
                    cursor: "pointer",
                    fontSize: 16,
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

    var exportDatalist = [];
    $scope.filterBydate = function (rep) {

        var filterData = [];
        angular.forEach(rep, function (value, key) {
            var obj = {};
            obj.totalOrder = value.totalOrder;
            obj.activeBrands = value.activeBrands;
            obj.activeRetailers = value.activeRetailers;
            obj.totalSale = value.TotalAmount;
            obj.dtt = value.year + "-" + value.month + "-" + value.day;
            obj.id = value.id;
            obj.name = value.name;
            obj.AgentName = value.AgentName;
            obj.OnlineOrderPercent = value.OnlineOrderPercent;

            filterData.push(obj);
            exportDatalist.push(obj);
        });
        if ($scope.type == "") {
            $scope.type = "1";
        };

        var fittypedata = $scope.filtertype(filterData);
        return fittypedata;
    }

    $scope.ExportList = function () {
        download($scope.FileUrl, "HubCityReport.xlsx");
        //console.log("'" + $scope.alaSqlQuery + "'")
        //alasql($scope.alaSqlQuery, [$scope.DataToExport]);
        //alasql('SELECT name,AgentName as [Agent Name],WarehouseName as [Warehouse Name],totalOrder as [Total Orders],activeBrands as [Active Brands],activeRetailers as [Order Retailers],TotalAmount as [Order Amount],New_SS_QA,FMCG,Test_Amit,new,new_Divyatest,FreqOfOrder as [Freq. Of Orders],AvgOrderValue as [Avg. Order Value],AvgLineItem as [Avg. Line Items],AppDownloads as [App Downloads], KisanKiranaAmount as [Kisan Kirana Sales],KisanKiranaPercent as [Kisan Kirana %],OnlineSales as [Online Sales],OnlineOrderPercent as [Online Order %],SignUp as [Signups Till date],TotalActiveRetailers as [Active Retailers Till date] INTO XLSX("HubCityReport.xlsx",{headers:true}) FROM ?', [$scope.DataToExport]);

    };


    function download(file, text) {

        //creating an invisible element 
        var element = document.createElement('a');
        element.setAttribute('download', text);
        element.setAttribute("href", file);
        document.body.appendChild(element);

        //onClick property 
        element.click();
        document.body.removeChild(element);
    }

    $scope.changeGraph = function (type) {

        $scope.type = type;
        $scope.filtertype();
    }
}]);