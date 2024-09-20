

(function () {
    'use strict';

    angular
        .module('app')
        .controller('weektimesheetController', weektimesheetController);

    weektimesheetController.$inject = ['$scope', 'tasktypesService', '$http', 'ngAuthSettings', 'ClientProjectService', 'logger'];

    function weektimesheetController($scope, tasktypesService, $http, ngAuthSettings, ClientProjectService, logger) {

        $scope.notify = function (type) {
            switch (type) {
                case "info":
                    return logger.log("Heads up! This alert needs your attention, but it's not super important.");
                case "success":
                    return logger.logSuccess("Well done! You successfully saved timesheet.");
                case "warning":
                    return logger.logWarning("Warning! Best check yo self, you're not looking too good.");
                case "error":
                    return logger.logError("Oh snap! Change a few things up and try submitting again.")
            }
        }
        console.log(" time sheet controller start loading");
        $scope.tasktypes = [];
        $scope.AllClientproject = '';
        $scope.today = new Date();
        $scope.weekGridRows = [];
        $scope.totalRows = 2;
        $scope.currentMonday = getMonday($scope.today);
        console.log("Current Monday" + $scope.currentMonday);

        function getMonday(d) {
            d = new Date(d);
            var day = d.getDay(),
                diff = d.getDate() - day + (day == 0 ? -6 : 1); // adjust when day is sunday
            return new Date(d.setDate(diff));
        }


        function getWeek(d) {
            var newArr = [];
            for (var i = 0; i < 5; i++) {

                newArr.push(i);
            }
        }

        //$scope.selected = void 0;
        //$scope.states = ["Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Dakota", "North Carolina", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming"];


        //function for get all Project
        ClientProjectService.getClientprojects().then(function (results) {
            console.log(results.data);
            $scope.AllClientproject = results.data;
            //$scope.AllClientproject = _.map(results.data, function (o) {
            //    console.log(o);
            //    var a = {};
            //    a = { "name": o.Id + '-' + o.Name };
            //    return a;
            //});
            //console.log($scope.AllClientproject);
        }, function (err) {
            console.log(err);
        });





        //This function for get all tasktype
        tasktypesService.gettasktypes().then(function (results) {
            $scope.tasktypes = results.data;

        }, function (error) {
            //alert(error.data.message);
        });



        //   remove row for week,day,month
        $scope.removeWeekRow = function (index) {
            console.log("Week Called");
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            console.log("Called delete week data");
            var turl = serviceBase + 'api/DeleteWeekEvent';
            //tasktypesService
            var row = $scope.weekGridRows[index];
            console.log(row);
            console.log($scope.weekGridRows);
            var gurl = serviceBase + 'api/DeleteWeekEvent';


            $http.post(turl, JSON.stringify(row)).success(function (data, status) {
                console.log(data);
                alert("done");
            })
            $scope.weekGridRows.splice(index, 1);
        }

        $scope.setProject = function (project, row) {

            console.log(project);
            console.log(row);
            row.projectid = project.projectid;
        }

        $scope.saveweekdata = function () {
            console.log("Week Called");
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            console.log("Called save week data");
            var turl = serviceBase + 'api/WeekEvents';
            //tasktypesService
            var d = { startdate: '' };
            console.log($scope.weekGridRows);
            var gurl = serviceBase + 'api/WeekEvents';
            var date = new Date(2015, 4, 27, 0, 0, 0, 0);
            d.startdate = date;
            console.log(d);
            $http.post(turl, JSON.stringify($scope.weekGridRows)).success(function (data, status) {
                console.log(data);
                $scope.notify('success');
                //alert("done");
            })
            //$http.get(gurl, d).success(function (data, status) {
            //    console.log(data);
            //    alert("done");
            //})
        }

        $scope.createWeekRows = function (startDate) {

            var today = startDate;


            var dd = today.getDate();
            var mm = today.getMonth() + 1; //January is 0!

            var yyyy = today.getFullYear();
            if (dd < 10) {
                dd = '0' + dd
            }
            if (mm < 10) {
                mm = '0' + mm
            }
            var todaystring = dd + '/' + mm + '/' + yyyy;
            //    console.log(todaystring);
            $http.get(serviceBase + 'api/weekevent?startdate=' + todaystring).then(function (results) {
                console.log("Data from server");
                console.log(results);
                $scope.weekGridRows = results.data;

                var d = new Date();

                var day = '';
                for (var i = 0; i < $scope.totalRows; i++) {




                    var eachRow =
                    {
                        "projectname": "",
                        "projectid": 0,
                        "clientproject": "",
                        "clientname": "",
                        "clientid": 0,
                        "tasktypeid": 0,
                        "tasktype": "",
                        "day": day,
                        "d1EventId": 0,
                        "d2EventId": 0,
                        "d3EventId": 0,
                        "d4EventId": 0,
                        "d5EventId": 0,
                        "d6EventId": 0,
                        "d7EventId": 0,
                        "d1": 0,
                        "d2": 0,
                        "d3": 0,
                        "d4": 0,
                        "d5": 0,
                        "d6": 0,
                        "d7": 0,
                        "startdate": todaystring,
                        "total": 0
                    };
                    //   console.log($scope.weekGridRows);
                    $scope.weekGridRows.push(eachRow);
                }
            });


            console.log("Complete add grid rows");
            console.log($scope.weekGridRows);
        }



        // add row for day,week,month 
        $scope.addWeekRow = function (index) {
            var eachRow =
            {
                "projectname": "A",
                "projectid": 1,
                "clientname": "test",
                "clientid": 1,
                "tasktypeid": 1,
                "tasktype": "Coding",
                "d1": 0,
                "d2": 0,
                "d3": 0,
                "d4": 0,
                "d5": 0,
                "d6": 0,
                "d7": 0,
                "total": 0,
                startdate: new Date()
            };

            //  console.log(eachRow);
            $scope.weekGridRows.push(eachRow);
        }



        //  Week bottom total
        $scope.d1Total = function () {
            //console.log("total week col");
            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.d1);
            });
            return total;
        }
        $scope.d2Total = function () {
            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.d2);
            });
            return total;
        }
        $scope.d3Total = function () {
            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.d3);
            });
            return total;
        }
        $scope.d4Total = function () {
            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.d4);
            });
            return total;
        }
        $scope.d5Total = function () {
            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.d5);
            });
            return total;
        }
        $scope.d6Total = function () {
            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.d6);
            });
            return total;
        }
        $scope.d7Total = function () {
            var total = 0;
            angular.forEach($scope.weekGridRows, function (row, index) {
                total += parseInt(row.d7);
            });
            return total;
        }
        //week all total
        $scope.WeekAllTotal = function () {
            return $scope.d1Total() + $scope.d2Total() + $scope.d3Total() + $scope.d4Total() + $scope.d5Total() + $scope.d6Total() + $scope.d7Total();
        }
        // for row and column total
        $scope.getWeekRowTotal = function (row) {
            //console.log("call Rolw");
            //console.log(row.d1);

            //   console.log(parseInt(row.d1) + parseInt(row.d2) + parseInt(row.d3) + parseInt(row.d4) + parseInt(row.d5) + parseInt(row.d6) + parseInt(row.d7));
            return parseInt(row.d1) + parseInt(row.d2) + parseInt(row.d3) + parseInt(row.d4) + parseInt(row.d5) + parseInt(row.d6) + parseInt(row.d7);
        }




        // function for current week
        $scope.thisWeek = function () {
            console.log("Current week");
            $scope.currentDate = new Date();
            $scope.currentMonday = getMonday($scope.currentDate);
            console.log($scope.currentMonday);
            var dd = moment($scope.currentMonday.toString());
            console.log(dd);
            $scope.textstring = "";
            $scope.d1 = dd.format("D MMM");
            $scope.textstring = dd.format("DD") + " - ";
            $scope.d2 = dd.add("days", 1).format("D MMM");
            $scope.d3 = dd.add("days", 1).format("D MMM");
            $scope.d4 = dd.add("days", 1).format("D MMM");
            $scope.d5 = dd.add("days", 1).format("D MMM");
            $scope.d6 = dd.add("days", 1).format("D MMM");
            $scope.d7 = dd.add("days", 1).format("D MMM");
            $scope.textstring += dd.format("D MMM YYYY");
            $scope.createWeekRows($scope.currentMonday);

        }

        //function for forward week,day,month
        $scope.forwardWeek = function () {
            console.log("Click forwardweek");


            var lastdate = moment($scope.currentMonday);
            console.log("Old Monday before increment");
            console.log(lastdate);
            var dd = lastdate.add("days", +7);
            var yy = new Date(dd.toDate().toString());
            $scope.currentMonday = yy;
            console.log("Current Monday beofre adding rows" + $scope.currentMonday);


            console.log(dd.format("DD MMM YYYY"));
            // frwd = dd.format("DD MMM YYYY");
            $scope.textstring = "";
            $scope.d1 = dd.format("D MMM");
            $scope.textstring = dd.format("DD") + " - ";
            $scope.d2 = dd.add("days", 1).format("D MMM");
            $scope.d3 = dd.add("days", 1).format("D MMM");
            $scope.d4 = dd.add("days", 1).format("D MMM");
            $scope.d5 = dd.add("days", 1).format("D MMM");
            $scope.d6 = dd.add("days", 1).format("D MMM");
            $scope.d7 = dd.add("days", 1).format("D MMM");
            $scope.textstring += dd.format("D MMM YYYY");
            $scope.createWeekRows($scope.currentMonday);
            console.log("Current Monday after adding rows" + $scope.currentMonday);

        }


        $scope.previousWeek = function () {

            console.log("Click Previousweek");
            console.log($scope.selectedFilterValue);
            var lastdate = moment($scope.currentMonday);
            var dd = lastdate.add("days", -7);
            var yy = new Date(dd.toDate().toString());
            $scope.currentMonday = yy;
            $scope.currentDate = dd.toDate();
            console.log(dd.format("DD MMM YYYY"));

            console.log("Current Date in previous week" + $scope.currentMonday);

            $scope.textstring = "";
            $scope.d1 = dd.format("D MMM");
            $scope.textstring = dd.format("DD") + " - ";
            $scope.d2 = dd.add("days", 1).format("D MMM");
            $scope.d3 = dd.add("days", 1).format("D MMM");
            $scope.d4 = dd.add("days", 1).format("D MMM");
            $scope.d5 = dd.add("days", 1).format("D MMM");
            $scope.d6 = dd.add("days", 1).format("D MMM");
            $scope.d7 = dd.add("days", 1).format("D MMM");
            $scope.textstring += dd.format("D MMM YYYY");
            $scope.createWeekRows($scope.currentMonday);

        }
        $scope.thisWeek();


//    $scope.textstring = function (d) {
//        var j = moment(m);
//        var startmonth = j.format('DD');
//        var dd = d.getDate();
//        var enddate = d.setDate(d + 6);
//        var dd1 = moment(enddate);
//        var mm = dd1.format('dd MM YYYY');
//        var finalstring = startmonth + ' - ' + mm;
//        console.log(finalstring);
//    }
        
    }
})();


