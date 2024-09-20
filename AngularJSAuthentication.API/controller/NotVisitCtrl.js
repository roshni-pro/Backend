

(function () {
    'use strict';

    angular
        .module('app')
        .controller('NotVisitCtrl', NotVisitCtrl);

    NotVisitCtrl.$inject = ['$scope', "$filter", "$http", "ngTableParams"];

    function NotVisitCtrl($scope, $filter, $http, ngTableParams) {

        $scope.datrange = '';
        $scope.datefrom = '';
        $scope.type = 1;
        $scope.dateto = '';
        $scope.Dayss = "";
        $scope.fulldata = [];
        $scope.dataselect1 = [];
        $scope.selectType = [
            { value: 1, text: "Date" },
            { value: 2, text: "Days" }

        ];
        $scope.selectedtypeChanged = function (data) {
            $scope.dataselect1 = data;
            console.log(data);

        }
        $scope.selectedDayChanged = function (Day) {

            alert(Day);
            $scope.Dayss = Day;
            console.log($scope.fulldata);
            //if (Day.length > 0) {
            //    $scope.dataselect1 = $scope.fulldata.filter((customer) => customer.Day === Day);
            //} else {
            //    $scope.dataselect1 = $scope.fulldata;
            //}
        }

        $(function () {

            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker24Hour: true,
                format: 'YYYY-MM-DD h:mm A'
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
            var date1 = yyyy + '-' + mm + '-' + dd;//instead of date used date1
            return date;
        }

        $scope.peoeplData = function () {
            $scope.dataselect = [];
            $http.get(serviceBase + 'api/NotVisit').then(function (results) {
                if (results.data != "null") {

                    $scope.dataselect = results.data;
                }
            });
        }

        $scope.TotalRetailer = function () {

            //$scope.Totalretailar;
            $scope.Totalretailar= ' ';
            $http.get(serviceBase + 'api/NotVisit/Total_Retailer').then(function (results) {
                if (results.data != "null") {

                    $scope.Totalretailar = results.data;
                }
            });
        }

        $scope.ActiveRetailer = function () {

            //$scope.Activeret;
            $scope.Activeret =' ';
            $http.get(serviceBase + 'api/NotVisit/Active_Retailer').then(function (results) {
                if (results.data != "null") {

                    $scope.Activeret = results.data;
                }
            });
        }

        $scope.TotalOrder = function () {

            //$scope.order;
            $scope.order=' ';
            $http.get(serviceBase + 'api/NotVisit/Total_Order').then(function (results) {
                if (results.data != "null") {

                    $scope.order = results.data;
                }
            });
        }

        $scope.peoeplData();
        $scope.TotalRetailer();
        $scope.TotalOrder();
        $scope.ActiveRetailer();
        $scope.examplemodel = [];
        $scope.exampledata = $scope.dataselect;
        $scope.examplesettings = {
            displayProp: 'DisplayName', idProp: 'PeopleID',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };
        var Allbydate = [];
        var AllbydateGenratedVisit = [];
        var AllbydateGenratedNotVisit = [];
        $scope.genereteReport = function () {

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
            var value = "";
            var ids = [];

            _.each($scope.examplemodel, function (o2) {
                console.log(o2);
                for (var i = 0; i < $scope.dataselect.length; i++) {
                    if ($scope.dataselect[i].PeopleID == o2.id) {
                        var Row =
                        {
                            "id": o2.id, "mob": $scope.dataselect[i].Mobile
                        };
                        ids.push(Row);
                    }
                }
            })
            var datatopost = {
                datefrom: $scope.dataforsearch.datefrom,
                dateto: $scope.dataforsearch.dateto,
                ids: ids,
                Day: $scope.Dayss,
                value: $scope.dataselect1
            }
            console.log(datatopost);
            $scope.datas = {};
            var url = serviceBase + "api/NotVisit/Search";
            $http.post(url, datatopost)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                        $scope.getChartOrder(null);
                    }
                    else {

                        Allbydate = data;
                        $scope.datas = data;
                        $scope.filtdata();
                    }
                });
            $scope.VisitedData = {};
            var url1 = serviceBase + "api/NotVisit/GetVisited";//instead of url used url1
            $http.post(url1, datatopost)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                        $scope.getChartOrder(null);
                    }
                    else {
                        AllbydateGenratedVisit = data;
                        $scope.VisitedData = data;
                        $scope.fillVisiteddata();
                    }
                });
            $scope.NotVisitedData = {};
            var url2 = serviceBase + "api/NotVisit/GetNotVisited";//instead of url used url2
            $http.post(url2, datatopost)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                        $scope.getChartOrder(null);
                    }
                    else {
                        AllbydateGenratedNotVisit = data;
                        $scope.NotVisitedData = data;
                        $scope.fillNotVisiteddata();
                    }
                });
        }

        $scope.filtdata = function () {
            $("#pierows").empty();
            for (var i = 0; i < Allbydate.length; i++) {
                if (Allbydate[i] != null && Allbydate[i].value == 2) {
                    var e = $('<div class="col-md-4" style="height:300px!important;"></div>');
                    var myid = "chartContainer" + i;
                    e.attr('id', myid);

                    $('#pierows').append(e);

                    var fd = [
                        { y: Allbydate[i].Day, label: "Day" },
                        { y: Allbydate[i].Totalbeat, label: "TotalBeat" },
                        { y: Allbydate[i].VisitedTotal, label: "Visited Total" },
                        { y: Allbydate[i].NotVisitedTotal, label: "Not Visited Total" },
                        { y: Allbydate[i].TotalRetailer, label: "Total Retailer" },
                        { y: Allbydate[i].ActiveRetailer, label: "Active Retailer" },
                        { y: Allbydate[i].TotalOrder, label: "Total Order" },
                        { y: Allbydate[i].TotalCost, label: "Total Cost" },
                        { y: Allbydate[i].NonvisitreasonPercent, label: "NoT Visit Reason Percent" }
                    ]
                    $scope.getChart2(fd, i, Allbydate[i].SalespersonName);
                }
                else {
                    if (Allbydate[i] != null && Allbydate[i].value == 1) {
                        var ee = $('<div class="col-md-4" style="height:300px!important;"></div>');//instead of e used ee
                        var myidd = "chartContainer" + i;
                        ee.attr('id', myidd);

                        $('#pierows').append(ee);

                        var fdd = [//instead of fd used fdd

                            { y: Allbydate[i].Totalbeat, label: "TotalBeat" },
                            { y: Allbydate[i].VisitedTotal, label: "Visited Total" },
                            { y: Allbydate[i].NotVisitedTotal, label: "Not Visited Total" },
                            { y: Allbydate[i].TotalRetailer, label: "Total Retailer" },
                            { y: Allbydate[i].ActiveRetailer, label: "Active Retailer" },
                            { y: Allbydate[i].TotalOrder, label: "Total Order" },
                            { y: Allbydate[i].TotalCost, label: "Total Cost" },
                            { y: Allbydate[i].NonvisitreasonPercent, label: "NoT Visit Reason Percent" }
                        ]
                        $scope.getChart(fdd, i, Allbydate[i].SalespersonName);
                    }
                }
            }
        }
        $scope.getChart2 = function (fdd, idd, SalespersonName) {
            var chart = new CanvasJS.Chart("chartContainer" + idd, {
                theme: "light2",
                title: {
                    text: SalespersonName + ",\n  Total Cost:" + fd[6].y
                },
                data: [{
                    type: "funnel",
                    indexLabel: "{label} [{y}]",
                    neckHeight: 0,
                    toolTipContent: "{label} - {y}",

                    dataPoints: [
                        { y: fd[0].y, label: "Day" },
                        { y: fd[1].y, label: "TotalBeat" },
                        { y: fd[2].y, label: "Visited Total" },
                        { y: fd[3].y, label: "Not Visited Total" },
                        { y: fd[4].y, label: "Total Retailer" },
                        { y: fd[5].y, label: "Active Retailer" },
                        { y: fd[6].y, label: "Total Order" }

                    ]
                }]
            });
            chart.render();

        }
        $scope.getChart = function (fd, idd, SalespersonName) {
            var chart = new CanvasJS.Chart("chartContainer" + idd, {
                theme: "light2",
                title: {
                    text: SalespersonName + ",\n  Total Cost:" + fd[6].y
                },
                data: [{
                    type: "funnel",
                    indexLabel: "{label} [{y}]",
                    neckHeight: 0,
                    toolTipContent: "{label} - {y}",

                    dataPoints: [

                        { y: fd[0].y, label: "TotalBeat" },
                        { y: fd[1].y, label: "Visited Total" },
                        { y: fd[2].y, label: "Not Visited Total" },
                        { y: fd[3].y, label: "Total Retailer" },
                        { y: fd[4].y, label: "Active Retailer" },
                        { y: fd[5].y, label: "Total Order" }
                    ]
                }]
            });
            chart.render();

        }
        $scope.fillVisiteddata = function () {
            $("#TotalVisitedpierows").empty();
            for (var i = 0; i < AllbydateGenratedVisit.length; i++) {
                if (AllbydateGenratedVisit[i] != null) {
                    var e = $('<div class="col-md-4" style="height:300px!important;"></div>');
                    var myid = "chartContainer1" + i;

                    e.attr('id', myid);

                    $('#TotalVisitedpierows').append(e);

                    var fd = [
                        { y: AllbydateGenratedVisit[i].price, label: "price" },
                        { y: AllbydateGenratedVisit[i].Other, label: "Other" },
                        { y: AllbydateGenratedVisit[i].ShopClosed, label: "ShopClosed" },
                        { y: AllbydateGenratedVisit[i].Notsatisfied, label: "Notsatisfied" }
                    ]
                    $scope.getChartVisiteddata(fd, i, AllbydateGenratedVisit[i].SalespersonName);
                }
            }

        }



        $scope.getChartVisiteddata = function (fd, idd, SalespersonName) {
            var chart = new CanvasJS.Chart("chartContainer1" + idd, {
                animationEnabled: true,
                title: {
                    text: SalespersonName
                },
                data: [{
                    type: "pie",
                    startAngle: 240,
                    yValueFormatString: "",
                    indexLabel: "{label} {y}",
                    dataPoints: [
                        { y: fd[0].y, label: "price" },
                        { y: fd[1].y, label: "Other" },
                        { y: fd[2].y, label: "ShopClosed" },
                        { y: fd[3].y, label: "Notsatisfied" }
                    ]
                }]
            });
            chart.render();

        }

        $scope.fillNotVisiteddata = function () {

            $("#TotalNotVisitedpierows").empty();
            for (var i = 0; i < AllbydateGenratedNotVisit.length; i++) {
                if (AllbydateGenratedNotVisit[i] != null) {
                    var e = $('<div class="col-md-4" style="height:300px!important;"></div>');
                    var myid = "chartContainer2" + i;

                    e.attr('id', myid);

                    $('#TotalNotVisitedpierows').append(e);

                    var fd = [
                        { y: AllbydateGenratedNotVisit[i].Lackoftime, label: "Lackoftime" },
                        { y: AllbydateGenratedNotVisit[i].NotAbleToFind, label: "NotAbleToFind" },
                        { y: AllbydateGenratedNotVisit[i].WrongTerritory, label: "WrongTerritory" },
                    ];
                    $scope.getChartNotVisiteddata(fd, i, AllbydateGenratedNotVisit[i].SalespersonName);
                }
            }

        }



        $scope.getChartNotVisiteddata = function (fd, idd, SalespersonName) {
            var chart = new CanvasJS.Chart("chartContainer2" + idd, {
                animationEnabled: true,
                title: {
                    text: SalespersonName
                },
                data: [{
                    type: "pie",
                    startAngle: 240,
                    yValueFormatString: "",
                    indexLabel: "{label} {y}",
                    dataPoints: [
                        { y: fd[0].y, label: "Lackoftime" },
                        { y: fd[1].y, label: "NotAbleToFind" },
                        { y: fd[2].y, label: "WrongTerritory" }
                    ]
                }]
            });
            chart.render();

        }
    }
})();

