

(function () {
    'use strict';

    angular
        .module('app')
        .controller('dashboardController', dashboardController);

    dashboardController.$inject = ['OrderMasterService', '$scope', '$http', "$filter", "ngTableParams", 'ngAuthSettings', 'authService', '$location'];

    function dashboardController(OrderMasterService, $scope, $http, $filter, ngTableParams, ngAuthSettings, authService, $location) {

        $scope.orders = [];
        OrderMasterService.getorders().then(function (results) {
            $scope.orders = results.data;
            console.log($scope.orders);
            console.log("$scope.orders");
        }, function (error) {
        });
        console.log("report Page is loading...");
        $scope.userfilter = false;
        $scope.currentMonth = new Date();
        $scope.currentSemimonth = new Date();
        $scope.currentWeek = new Date();
        $scope.currentQuarter = new Date();
        $scope.currentYear = new Date();
        $scope.currentDay = new Date();
        $scope.weekGridRows = [];
        $scope.clientGridRows = [];
        $scope.projectGridRows = [];
        $scope.taskGridRows = [];
        $scope.selectedFilterValue = "";
        $scope.userid = 0;
        $scope.pieChart = {};
        $scope.startDate = "";
        $scope.endDate = "";


        if (authService.authentication.isAuth == false) {
            console.log("Not authenticated");
            $location.path("/pages/signin");
        } else {
            if (authService.authentication.rolenames.indexOf("HQ Master login")==-1) {
                console.log("user id is :" + authService.authentication.userid);
                $scope.userid = authService.authentication.userid;
            }
            console.log("Auth value" + authService.authentication.isAuth);
        }
        $scope.loadperson = function (id) {
            console.log("Draw person");
            $scope.userid = id;
            console.log($scope.userid);
            $scope.loadperson = true;
            var dd = moment($scope.startDate.toString());
            var yy = moment($scope.endDate.toString());
            console.log(dd.format("DD/MM/YYYY"));
            console.log(yy.format("DD/MM/YYYY"));
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), $scope.selectedFilterValue);
            //   $scope.drawCircle($scope.startDate, $scope.endDate, $scope.selectedFilterValue)
        }
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
            if ($scope.selectedItem == "Day") {
                $scope.thisDay();
            } else if ($scope.selectedItem == "Week") {
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
            if ($scope.selectedItem == "Day") {
                $scope.previousDay();
            } else if ($scope.selectedItem == "Week") {
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
            if ($scope.selectedItem == "Day") {
                $scope.forwardDay();
            } else if ($scope.selectedItem == "Week") {
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
            if ($scope.selectedItem == "Day") {
                $scope.thisDay();
            } else if ($scope.selectedItem == "Week") {
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

            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            $scope.currentSemimonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "SemiMonth");
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
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            $scope.currentSemimonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "SemiMonth");
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
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            $scope.currentSemimonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "SemiMonth");
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

            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");

            $scope.currentYear = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Year");
        }
        $scope.forwardYear = function () {



            var today = $scope.currentYear;

            var firstDay = new Date(today.getFullYear() + 1, 0, 1);

            var lastDay = new Date(today.getFullYear() + 2, 0, 0);


            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());

            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");

            $scope.currentYear = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Year");
        }
        $scope.previousYear = function () {



            var today = $scope.currentYear;

            var firstDay = new Date(today.getFullYear() - 1, 0, 1);

            var lastDay = new Date(today.getFullYear(), 0, 0);


            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());

            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            $scope.currentYear = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Year");
        }

        $scope.thisDay = function () {
            $scope.selectedFilterValue = "Daily";
            $scope.selectedItem = "Day";
            console.log("Current week");
            $scope.currentDate = new Date();
            $scope.currentDay = $scope.currentDate;

            $scope.startDate = $scope.currentDate;
            $scope.endDate = $scope.currentDate;
            var dd = moment($scope.currentDay.toString());


            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = dd.format("DD/MM/YYYY");
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += dd.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), dd.format("DD/MM/YYYY"), "Day");

            //   $scope.createWeekRows($scope.currentWeek);

        }

        $scope.forwardDay = function () {
            console.log("Click forwardweek");


            var lastdate = moment($scope.currentDay);
            console.log("Old current date before increment");
            console.log(lastdate);
            var dd = lastdate.add("days", +1);
            var yy = new Date(dd.toDate().toString());
            var xx = moment(yy);

            $scope.startDate = xx.format("DD/MM/YYYY");
            $scope.endDate = xx.format("DD/MM/YYYY");
            $scope.currentDay = yy;
            console.log("Current day beofre adding rows" + $scope.currentDay);


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

            $scope.textstring += dd.format("D MMM YYYY");
            //$scope.createWeekRows($scope.currentWeek);
            $scope.drawCircle(xx.format("DD/MM/YYYY"), dd.format("DD/MM/YYYY"), "Day");
            console.log("Current day after adding rows" + $scope.currentDay);
            //$scope.drawCircle($scope.currentWeek);
        }

        $scope.previousDay = function () {

            console.log("Click Previousweek");
            console.log($scope.selectedFilterValue);
            var lastdate = moment($scope.currentDay);
            var dd = lastdate.add("days", -1);
            var yy = new Date(dd.toDate().toString());
            var xx = moment(yy);
            $scope.currentDay = yy;
            $scope.currentDate = dd.toDate();
            console.log(dd.format("DD MMM YYYY"));
            $scope.startDate = xx.format("DD/MM/YYYY");
            $scope.endDate = xx.format("DD/MM/YYYY");
            console.log("Current Date in previous week" + $scope.currentDay);

            $scope.textstring = "";
            $scope.d1 = dd.format("D MMM");
            $scope.textstring = dd.format("DD") + " - ";


            $scope.textstring += dd.format("D MMM YYYY");
            $scope.drawCircle(xx.format("DD/MM/YYYY"), dd.format("DD/MM/YYYY"), "Day");
            //  $scope.createWeekRows($scope.currentMonday);

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

            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Week");

            //   $scope.createWeekRows($scope.currentWeek);

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
            $scope.currentDate = dd.toDate();
            console.log("Current Monday beofre adding rows" + $scope.currentWeek);


            console.log(dd.format("DD MMM YYYY"));
            // frwd = dd.format("DD MMM YYYY");
            $scope.textstring = "";
            //$scope.d1 = dd.format("D MMM");
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");

            $scope.textstring = dd.format("DD") + " - ";
            $scope.d7 = dd.add("days", 7).format("D MMM");
            $scope.textstring += dd.format("D MMM YYYY");
            //$scope.createWeekRows($scope.currentWeek);
            $scope.drawCircle(xx.format("DD/MM/YYYY"), dd.format("DD/MM/YYYY"), "Week");
            console.log("Current Monday after adding rows" + $scope.currentWeek);
            //$scope.drawCircle($scope.currentWeek);
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
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = xx.format("DD/MM/YYYY");
            $scope.textstring = "";
            $scope.d1 = dd.format("D MMM");
            $scope.textstring = dd.format("DD") + " - ";

            $scope.d7 = dd.add("days", 7).format("D MMM");
            $scope.textstring += dd.format("D MMM YYYY");
            $scope.drawCircle(xx.format("DD/MM/YYYY"), dd.format("DD/MM/YYYY"), "Week");
            //  $scope.createWeekRows($scope.currentMonday);

        }

        $scope.thisMonth = function () {
            $scope.selectedFilterValue = "Month";
            var date = new Date();
            var firstDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth(), 1);
            var lastDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth() + 1, 0);
            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());

            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            $scope.currentMonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Month");
        }

        $scope.forwardMonth = function () {
            console.log("Click forwardweek");



            var firstDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth() + 1, 1);
            var lastDay = new Date($scope.currentMonth.getFullYear(), $scope.currentMonth.getMonth() + 2, 0);
            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.currentMonth = firstDay;
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");

            console.log("Current Monday after adding rows" + $scope.currentMonth);
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Month");
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
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");

            console.log("Current Monday after adding rows" + $scope.currentMonth);
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Month");
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
                var firstDay4= new Date($scope.currentQuarter.getFullYear(), 10, 1);
            }
            var lastDay = new Date(firstDay.getFullYear(), firstDay.getMonth() + 4, 0);

            $scope.currentQuarter = firstDay;
            var dd = moment(firstDay.toString());
            var yy = moment(lastDay.toString());
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");

            $scope.currentMonth = firstDay;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Quarter");
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
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            $scope.currentQuarter = ff;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");

            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Quarter");
        }

        $scope.previousQuarter = function () {
            console.log("previous quarter");
            console.log("first day of last quarter");
            console.log($scope.currentQuarter);

            var ff = new Date($scope.currentQuarter.getFullYear(), $scope.currentQuarter.getMonth() - 4, 1);

            console.log("first day of current quarter");
            console.log(ff);

            var lastDay = new Date(ff.getFullYear(), ff.getMonth() + 4, 0);
            console.log("last day of current quarter");
            console.log(lastDay);

            var dd = moment(ff.toString());
            var yy = moment(lastDay.toString());
            $scope.startDate = dd.format("DD/MM/YYYY");
            $scope.endDate = yy.format("DD/MM/YYYY");
            $scope.currentQuarter = ff;
            console.log(dd);
            $scope.textstring = "";

            $scope.textstring = dd.format("DD MMM") + " - ";

            $scope.textstring += yy.format("D MMM YYYY");
            $scope.drawCircle(dd.format("DD/MM/YYYY"), yy.format("DD/MM/YYYY"), "Quarter");
        }


        $scope.drawCircle = function (startdate, enddate, range) {
            console.log(startdate);
            console.log(enddate);

            $http.get(serviceBase + 'api/ResourceReport?startdate=' + startdate + '&enddate=' + enddate + '&userid=' + $scope.userid + '&gridfilter=Staff').then(function (results) {
                console.log("Data from server");
                console.log(results);
                $scope.weekGridRows = results.data.StaffReport;
                $scope.clientGridRows = results.data.ClientReport;
                $scope.projectGridRows = results.data.ProjectReport;
                $scope.taskGridRows = results.data.TaskReport;
                $scope.callmethod();

                $scope.percent = $scope.billableHours() / $scope.weekTotal() * 100;
                var child = document.getElementById('circles-1'),

                    circles = [];
                circle = Circles.create({
                    id: child.id,
                    value: $scope.percent,
                    radius: 38.55,
                    width: 10,
                    colors: ['#b5e5a5', '#41b419']
                });
                for (var i = 0; i < circles.length; i++) {

                    circles[i].updateRadius(25);
                }

                circles.push(circle);

                //$http.get(serviceBase + 'api/ResourceReport?startdate=' + startdate + '&enddate=' + enddate + "&userid=" + $scope.userid + "&gridfilter=Client").then(function (results) {
                //    console.log("Data from server");
                //    console.log(results);
                //    if (results) {
                //        $scope.clientGridRows = results.data;
                //    }

                //});

                //$http.get(serviceBase + 'api/ResourceReport?startdate=' + startdate + '&enddate=' + enddate + "&userid=" + $scope.userid + "&gridfilter=Project").then(function (results) {
                //    console.log("Data from server");
                //    console.log(results);
                //    if (results) {
                //        $scope.projectGridRows = results.data;
                //    }

                //});

                //$http.get(serviceBase + 'api/BarChart?startdate=' + startdate + '&enddate=' + enddate + "&userid=" + $scope.userid).then(function (results) {
                console.log("Data from server");
                console.log(results);
                $scope.barRows = results.data.BarChartReport;
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
                    if (results) {
                        $scope.chart.setData(results.data.BarChartReport);
                        $scope.chart.redraw();
                    }
                }


                //});

                //$http.get(serviceBase + 'api/PieChart?startdate=' + startdate + '&enddate=' + enddate + "&userid=" + $scope.userid).then(function (results) {
                console.log("Data from server");
                console.log(results.data);
                //  $scope.barRows = results.data;

                if (results.data) {
                    //  $scope.pieChart.data = results.data;
                    $.plot($("#piechart"), results.data.PieChartReport, {
                        series: {
                            pie: {
                                show: true
                            }
                        },
                        legend: {
                            labelBoxBorderColor: "none"
                        }
                    });

                }
                                                          
                //    });

            });
        }
        $scope.thisDay();

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
            $scope.search(), $scope.select(1);
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
    }
})();
