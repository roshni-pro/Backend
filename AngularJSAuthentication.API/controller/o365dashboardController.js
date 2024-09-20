

(function () {
    'use strict';

    angular
        .module('app')
        .controller('o365dashboardController', o365dashboardController);

    o365dashboardController.$inject = ['$scope', 'peoplesService', '$http', "$filter", "ngTableParams", 'ngAuthSettings'];

    function o365dashboardController($scope, peoplesService, $http, $filter, ngTableParams, ngAuthSettings) {

        console.log("report Page is loading...");
        $scope.peoples = [];
        $scope.currentMonth = new Date();
        $scope.currentSemimonth = new Date();
        $scope.currentWeek = new Date();
        $scope.currentQuarter = new Date();
        $scope.currentYear = new Date();
        $scope.weekGridRows = [];
        $scope.clientGridRows = [];
        $scope.projectGridRows = [];
        $scope.selectedFilterValue = "";
        $scope.weekTotal = function () {

            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.Hours);
            });
            return total;
        }
        $scope.billableHours = function () {

            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.Billable);
            });
            return total;
        }
        $scope.nonBillableHours = function () {

            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.NonBillable);
            });
            return total;
        }
        $scope.billableAmount = function () {

            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += (parseInt(row.Billable) * parseInt(row.Rate));
            });
            return total;
        }
        $scope.nonBillableAmount = function () {

            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {

                total += (parseInt(row.NonBillable) * parseInt(row.Rate));
            });
            return total;
        }
        $scope.item = '';

        $scope.update = function () {
            console.log($scope.selectedItem);
            if ($scope.selectedItem == "Week") {
                $scope.thisWeek();
            } else if ($scope.selectedItem == "Month") {
                $scope.thisMonth();
            } else if ($scope.selectedItem == "Quarter") {
                $scope.thisQuarter();
            }

            else if ($scope.selectedItem == "Semimonthly") {
                $scope.thisSemiMonth();
            }
            else if ($scope.selectedItem == "Year") {
                $scope.thisYear();
            }
        }
        $scope.previous = function () {
            if ($scope.selectedItem == "Week") {
                $scope.previousWeek();
            } else if ($scope.selectedItem == "Month") {
                $scope.previousMonth();
            } else if ($scope.selectedItem == "Quarter") {
                $scope.previousQuarter();
            } else if ($scope.selectedItem == "Semimonthly") {
                $scope.previousSemiMonth();
            }
            else if ($scope.selectedItem == "Year") {
                $scope.previousYear();
            }
        }
        $scope.next = function () {
            if ($scope.selectedItem == "Week") {
                $scope.forwardWeek();
            } else if ($scope.selectedItem == "Month") {
                $scope.forwardMonth();
            } else if ($scope.selectedItem == "Quarter") {
                $scope.forwardQurater();
            } else if ($scope.selectedItem == "Semimonthly") {
                $scope.forwardSemiMonth();
            }
            else if ($scope.selectedItem == "Year") {
                $scope.forwardYear();
            }

        }
        $scope.curret = function () {
            if ($scope.selectedItem == "Week") {
                $scope.thisWeek();
            } else if ($scope.selectedItem == "Month") {
                $scope.thisMonth();
            }
            else if ($scope.selectedItem == "Quarter") {
                $scope.thisQuarter();
            }

            else if ($scope.selectedItem == "Semimonthly") {
                $scope.thisSemiMonth();
            }
            else if ($scope.selectedItem == "Year") {
                $scope.thisYear();
            }
        }
        $scope.thisSemiMonth = function () {
            $scope.selectedFilterValue = "SemiMonthly";
            var date = new Date();
            var firstDay = new Date();
            var lastDay = new Date();
            if (date.getDate() < 15) {
                firstDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth(), 1);
                lastDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth(), 14);
            } else {
                firstDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth(), 15);
                lastDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth() + 1, 0);
            }


            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentSemimonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }
        $scope.forwardSemiMonth = function () {

            var date = $scope.currentSemimonth;
            var firstDay = new Date();
            var lastDay = new Date();
            if (date.getDate() < 15) {
                firstDay = new Date($scope.currentSemimonth.getFullYear(), $scope.currentSemimonth.getMonth(), 15);
                lastDay = new Date($scope.currentSemimonth.getFullYear(), $scope.currentSemimonth.getMonth() + 1, 0);

            } else {
                firstDay = new Date($scope.currentSemimonth.getFullYear(), $scope.currentSemimonth.getMonth() + 1, 1);
                lastDay = new Date($scope.currentSemimonth.getFullYear(), $scope.currentSemimonth.getMonth() + 1, 14);
            }


            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentSemimonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }
        $scope.previousSemiMonth = function () {

            var date = $scope.currentSemimonth;
            var firstDay = new Date();
            var lastDay = new Date();
            if (date.getDate() < 15) {
                firstDay = new Date($scope.currentSemimonth.getFullYear(), $scope.currentSemimonth.getMonth() - 1, 15);
                lastDay = new Date($scope.currentSemimonth.getFullYear(), $scope.currentSemimonth.getMonth(), 0);

            } else {
                firstDay = new Date($scope.currentSemimonth.getFullYear(), $scope.currentSemimonth.getMonth(), 1);
                lastDay = new Date($scope.currentSemimonth.getFullYear(), $scope.currentSemimonth.getMonth(), 14);

            }


            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentSemimonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }

        function getMonday(d) {
            d = new Date(d);
            var day = d.getDay(),
                diff = d.getDate() - day + (day == 0 ? -6 : 1); // adjust when day is sunday
            return new Date(d.setDate(diff));
        }


        $scope.thisYear = function () {

            $scope.selectedFilterValue = "Year";

            var today = new Date();

            var firstDay = new Date(today.getFullYear(), 0, 1);

            var lastDay = new Date(today.getFullYear() + 1, 0, 0);


            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentYear = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }
        $scope.forwardYear = function () {



            var today = $scope.currentYear;

            var firstDay = new Date(today.getFullYear() + 1, 0, 1);

            var lastDay = new Date(today.getFullYear() + 2, 0, 0);


            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentYear = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }
        $scope.previousYear = function () {



            var today = $scope.currentYear;

            var firstDay = new Date(today.getFullYear() - 1, 0, 1);

            var lastDay = new Date(today.getFullYear(), 0, 0);


            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentYear = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }

        // function for current week
        $scope.thisWeek = function () {
            $scope.selectedFilterValue = "Week";
            $scope.selectedItem = "Week";
            console.log("Current week");
            $scope.currentDate = new Date();
            $scope.currentWeek = getMonday($scope.currentDate);
            var lastdate = moment($scope.currentWeek);
            console.log($scope.currentWeek);

            var dd = moment($scope.currentWeek.toString());
            var yy = lastdate.add("days", +7);


            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));

            //   $scope.createWeekRows($scope.currentWeek);

        }

        $scope.drawWeekCircle = function (startdate, enddate) {
            console.log(startdate);
            console.log(enddate);
            //var today = date;


            //var dd = today.getDate();
            //var mm = today.getMonth() + 1; //January is 0!

            //var yyyy = today.getFullYear();
            //if (dd < 10) {
            //    dd = '0' + dd
            //}
            //if (mm < 10) {
            //    mm = '0' + mm
            //}
            //   var todaystring = dd + '/' + mm + '/' + yyyy;
            $http.get(serviceBase + 'api/ResourceReport?startdate=' + startdate + '&enddate=' + enddate + '&userid=0&gridfilter=Staff').then(function (results) {
                console.log("Data from server");
                console.log(results);
                $scope.weekGridRows = results.data;
                $scope.callmethod();


                $scope.percent = $scope.billableHours() / $scope.weekTotal() * 100;
                var child = document.getElementById('circles-1'),


                    circle = Circles.create({
                        id: child.id,
                        value: $scope.percent,
                        radius: 38.55,
                        width: 10,
                        colors: ['#b5e5a5', '#41b419']
                    });
                $http.get(serviceBase + 'api/ResourceReport?startdate=' + startdate + '&enddate=' + enddate + "&userid=0&&gridfilter=Client").then(function (results) {
                    console.log("Data from server");
                    console.log(results);
                    $scope.clientGridRows = results.data;
                    //$scope.callmethod();

                });
                $http.get(serviceBase + 'api/ResourceReport?startdate=' + startdate + '&enddate=' + enddate + "&userid=0&&gridfilter=Project").then(function (results) {
                    console.log("Data from server");
                    console.log(results);
                    $scope.projectGridRows = results.data;

                });
                $http.get(serviceBase + 'api/ResourceReport?startdate=' + startdate + '&enddate=' + enddate + "&userid=0&&gridfilter=Project").then(function (results) {
                    console.log("Data from server");
                    console.log(results);
                    $scope.projectGridRows = results.data;

                });
                $http.get(serviceBase + 'api/BarChart?startdate=' + startdate + '&enddate=' + enddate + "&userid=0").then(function (results) {
                    console.log("Data from server");
                    console.log(results);
                    $scope.barRows = results.data;
                    if (!$scope.chart) {
                        $scope.chart = Morris.Bar({
                            element: 'barchart',
                            data: $scope.barRows,
                            xkey: 'Name',
                            barColors: ['#41b419', '#b5e5a5'],
                            ykeys: ['Billable', 'NonBillable'],

                            labels: ['Billable', 'NonBillable']
                        });
                    } else {
                        console.log('setting chart values');
                        $scope.chart.setData(results.data);
                        $scope.chart.redraw();
                    }


                });

                $http.get(serviceBase + 'api/PieChart?startdate=' + startdate + '&enddate=' + enddate + "&userid=0").then(function (results) {
                    console.log("Data from server");
                    console.log(results);
                    //  $scope.barRows = results.data;
                    if (!$scope.float) {
                        $scope.float = Morris.Donut({
                            element: 'floatchart',
                            data: results.data,
                            colors: ["#176799", "#2F87B0", "#42A4BB", "#5BC0C4", "#78D6C7"]
                        });
                    } else {
                        $scope.float.setData(results.data);
                        $scope.float.redraw();

                    }

                });

                circles.push(circle);

            });
        }

        //function for forward week,day,month
        $scope.forwardWeek = function () {
            console.log("Click forwardweek");


            var lastdate = moment($scope.currentWeek);
            console.log("Old Monday before increment");
            console.log(lastdate);
            var dd = lastdate.add("days", +7);
            var yy = new Date(dd.toDate().toString());
            var xx = moment(yy);
            $scope.currentWeek = yy;
            console.log("Current Monday beofre adding rows" + $scope.currentWeek);


            console.log(dd.format("DD MMM YYYY"));
            // frwd = dd.format("DD MMM YYYY");
            $scope.textstring = "";
            //$scope.d1 = dd.format("D MMM");
            $scope.textstring = dd.format("DD") + " - ";
            //$scope.d2 = dd.add("days", 1).format("D MMM");
            //$scope.d3 = dd.add("days", 1).format("D MMM");
            //$scope.d4 = dd.add("days", 1).format("D MMM");
            //$scope.d5 = dd.add("days", 1).format("D MMM");
            //$scope.d6 = dd.add("days", 1).format("D MMM");
            $scope.d7 = dd.add("days", 7).format("D MMM");
            $scope.textstring += dd.format("D MMM YYYY");
            //$scope.createWeekRows($scope.currentWeek);
            $scope.drawWeekCircle(xx.format("DD/MM/YYYY"), dd.format("DD/MM/YYYY"));
            console.log("Current Monday after adding rows" + $scope.currentWeek);
            //$scope.drawWeekCircle($scope.currentWeek);
        }

        $scope.previousWeek = function () {

            console.log("Click Previousweek");
            console.log($scope.selectedFilterValue);
            var lastdate = moment($scope.currentWeek);
            var dd = lastdate.add("days", -7);
            var yy = new Date(dd.toDate().toString());
            var xx = moment(yy);
            $scope.currentWeek = yy;
            $scope.currentDate = dd.toDate();
            console.log(dd.format("DD MMM YYYY"));

            console.log("Current Date in previous week" + $scope.currentWeek);

            $scope.textstring = "";
            $scope.d1 = dd.format("D MMM");
            $scope.textstring = dd.format("DD") + " - ";

            $scope.d7 = dd.add("days", 7).format("D MMM");
            $scope.textstring += dd.format("D MMM YYYY");
            $scope.drawWeekCircle(xx.format("DD/MM/YYYY"), dd.format("DD/MM/YYYY"));
            //  $scope.createWeekRows($scope.currentMonday);

        }

        $scope.thisMonth = function () {
            $scope.selectedFilterValue = "Month";
            var date = new Date();
            var firstDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth(), 1);
            var lastDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth() + 1, 0);
            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentMonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }

        $scope.forwardMonth = function () {
            console.log("Click forwardweek");



            var firstDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth() + 1, 1);
            var lastDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth() + 2, 0);
            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentMonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");

            console.log("Current Monday after adding rows" + $scope.currentMonth);
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }

        $scope.previousMonth = function () {
            console.log("Click forwardweek");



            var firstDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth() - 1, 1);
            var lastDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth(), 0);
            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentMonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");

            console.log("Current Monday after adding rows" + $scope.currentMonth);
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }

        $scope.thisQuarter = function () {
            $scope.selectedFilterValue = "Quarter";
            var date = new Date();

            var today = new Date();
            var quarter = Math.floor((today.getMonth() + 3) / 3);
            var firstDay = new Date();
            if (quarter == 1) {
                var firstDay1 = new Date($scope.currentQuarter.getFullYear(), 1, 1);

            } else if (quarter == 2) {
                var firstDay2 = new Date($scope.currentQuarter.getFullYear(), 4, 1);
            }
            else if (quarter == 3) {
                var firstDay3 = new Date($scope.currentQuarter.getFullYear(), 7, 1);
            }
            else if (quarter == 3) {
                var firstDay4 = new Date($scope.currentQuarter.getFullYear(), 10, 1);
            }
            var lastDay = new Date(firstDay.getFullYear(), firstDay.getMonth() + 4, 0);

            $scope.currentQuarter = firstDay;
            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentMonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }

        $scope.forwardQurater = function () {

            console.log("forward quarter");
            console.log("first day of last quarter");
            console.log($scope.currentQuarter);

            var ff = new Date($scope.currentQuarter.getFullYear(), $scope.currentQuarter.getMonth() + 4, 1);

            console.log("first day of current quarter");
            console.log(ff);

            var lastDay = new Date(ff.getFullYear(), ff.getMonth() + 4, 0);
            console.log("last day of current quarter");
            console.log(lastDay);

            var dd = moment(ff.toString());
            var yy = moment(lastDay.toString());
            $scope.currentQuarter = ff;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");

            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }

        $scope.previousQuarter = function () {
            console.log("previous quarter");
            console.log("first day of last quarter");
            console.log($scope.currentQuarter);

            var ff = new Date($scope.currentQuarter.getFullYear(), $scope.currentQuarter.getMonth() - 4, 1);

            console.log("first day of current quarter");
            console.log(ff);
            //var quarter = Math.floor((ff.getMonth() + 3) / 3);

            //if (quarter == 1) {
            //    var firstDay = new Date(ff.getFullYear(), 1, 1);

            //} else if (quarter == 2) {
            //    var firstDay = new Date(ff.getFullYear(), 4, 1);
            //}
            //else if (quarter == 3) {
            //    var firstDay = new Date(ff.getFullYear(), 7, 1);
            //}
            //else if (quarter == 3) {
            //    var firstDay = new Date(ff.getFullYear(), 10, 1);
            //}
            var lastDay = new Date(ff.getFullYear(), ff.getMonth() + 4, 0);
            console.log("last day of current quarter");
            console.log(lastDay);

            var dd = moment(ff.toString());
            var yy = moment(lastDay.toString());
            $scope.currentQuarter = ff;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawWeekCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"));
        }

        $scope.getTeamMembers = function () {
            //$scope.peoples = [];
            peoplesService.getpeoples().then(function (results) {

                $scope.peoples = results.data;
                console.log("Got people collection");
                console.log($scope.peoples);

            }, function (error) {
                //alert(error.data.message);
            });
        }
        //var strDtStart = moment().subtract(1, 'days').format('YYYY-MM-DD');
        //var strDtEnd = moment().subtract(1, 'days').format('YYYY-MM-DD');
        //var start = new Date();
        //var end = new Date();

        //$('#reportrange span').html(moment(start).format('MMMM D, YYYY') + ' - ' + moment(end).format('MMMM D, YYYY'));
        //$('#reportrange').daterangepicker({ startDate: moment(start).format('MM-DD-YYYY'), endDate: moment(end).format('MM-DD-YYYY') });
        $scope.date = {
            startDate: moment().subtract("days", 1),
            endDate: moment()
        };
        $scope.date2 = {
            startDate: moment().subtract("days", 1),
            endDate: moment()
        };
        $scope.opts = {
            locale: {
                applyLabel: 'Submit',
                cancelLabel: 'Cancel',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
            },
        };
        //Watch for date changes
        $scope.$watch('date', function (newDate) {
            console.log('New date set: ', newDate);
        }, false);

        $scope.getTeamMembers();

        $scope.thisWeek();
        console.log("People");
        console.log($scope.peoples);
        $scope.example14model = [];
        $scope.example14data = $scope.peoples;
        //[
        //{ id: 1, label: "David" },
        //{ id: 2, label: "Jhon" },
        //{ id: 3, label: "Lisa" },
        //{ id: 4, label: "Nicole" },
        //{ id: 5, label: "Danny" },
        //{ id: 6, label: "Dan" },
        //{ id: 7, label: "Dean" },
        //{ id: 8, label: "Adam" },
        //{ id: 9, label: "Uri" },
        //{ id: 10, label: "Phil" }];

        $scope.example14settings = {
            displayProp: 'DisplayName', idProp: 'PeopleID',
            scrollableHeight: '200px',
            scrollable: true,
            enableSearch: true
        };


        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.weekGridRows;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";


            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.weekGridRows = [];
            $scope.search(); $scope.select(1);
        }

        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.weekGridRows = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {
            console.log("search");
            console.log($scope.stores);
            console.log($scope.searchKeywords);

            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            console.log("order"); console.log($scope.stores);
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }


        //$scope.callmethod = function () {

        //    var init;
        //    return $scope.stores = $scope.weekGridRows,

        //        $scope.searchKeywordsStaff = "",
        //        $scope.filteredStores = [],
        //        $scope.row = "",

        //        $scope.select = function (page) {
        //            var end, start; console.log("select"); console.log($scope.stores);
        //            return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.weekGridRows = $scope.filteredStores.slice(start, end)
        //        },

        //        $scope.onFilterChange = function () {
        //            console.log("onFilterChange"); console.log($scope.stores);
        //            return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
        //        },

        //        $scope.onNumPerPageChange = function () {
        //            console.log("onNumPerPageChange"); console.log($scope.stores);
        //            return $scope.select(1), $scope.currentPage = 1
        //        },

        //        $scope.onOrderChange = function () {
        //            console.log("onOrderChange"); console.log($scope.stores);
        //            return $scope.select(1), $scope.currentPage = 1
        //        },

        //        $scope.search = function () {
        //            console.log("search");
        //            console.log($scope.stores);
        //            console.log($scope.searchKeywordsStaff);

        //            return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywordsStaff), $scope.onFilterChange()
        //        },

        //        $scope.order = function (rowName) {
        //            console.log("order"); console.log($scope.stores);
        //            return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
        //        },

        //        $scope.numPerPageOpt = [3, 5, 10, 20],
        //        $scope.numPerPage = $scope.numPerPageOpt[2],
        //        $scope.currentPage = 1,
        //        $scope.weekGridRows = [],
        //        (init = function () {
        //            return $scope.search(), $scope.select($scope.currentPage)
        //        })

        //    ()


        //}
    }
})();