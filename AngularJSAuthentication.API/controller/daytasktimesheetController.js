


(function () {
    'use strict';

    angular
        .module('app')
        .controller('daytasktimesheetController', daytasktimesheetController);

    daytasktimesheetController.$inject = ['$scope', 'tasktypesService', '$http', 'ngAuthSettings', 'ClientProjectService'];

    function daytasktimesheetController($scope, tasktypesService, $http, ngAuthSettings, ClientProjectService) {

        console.log(" time sheet controller start loading");
        $scope.tasktypes = [];
        $scope.AllClientproject = '';
        $scope.today = new Date();
        $scope.dayGridRows = [];
        $scope.totalRows = 2;
        $scope.currentDay = $scope.today;


        function getMonday(d) {
            d = new Date(d);
            var day = d.getDay(),
                diff = d.getDate() - day + (day == 0 ? -6 : 1); // adjust when day is sunday
            return new Date(d.setDate(diff));
        }

        function getday(d) {
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



        //   remove row for day,day,month
        $scope.removedayRow = function (index) {
            console.log("day Called");
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            console.log("Called delete day data");
            var turl = serviceBase + 'api/DeletedayEvent';
            //tasktypesService
            var row = $scope.dayGridRows[index];
            console.log(row);
            console.log($scope.dayGridRows);
            var gurl = serviceBase + 'api/DeletedayEvent';


            $http.post(turl, JSON.stringify(row)).success(function (data, status) {
                console.log(data);
                alert("done");
            })
            $scope.dayGridRows.splice(index, 1);
        }


        $scope.savedaydata = function () {
            console.log("day Called");
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            console.log("Called save day data");
            var turl = serviceBase + 'api/dayEvents';
            //tasktypesService
            var d = { startdate: '' };
            console.log($scope.dayGridRows);
            var gurl = serviceBase + 'api/dayEvents';
            var date = new Date(2015, 4, 27, 0, 0, 0, 0);
            d.startdate = date;
            console.log(d);
            $http.post(turl, JSON.stringify($scope.dayGridRows)).success(function (data, status) {
                console.log(data);
                alert("done");
            })
            //$http.get(gurl, d).success(function (data, status) {
            //    console.log(data);
            //    alert("done");
            //})
        }

        $scope.createdayRows = function (startDate) {

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
            $http.get(serviceBase + 'api/DayTaskEvent?startdate=' + todaystring).then(function (results) {
                console.log("Data from server");
                console.log(results);
                $scope.dayGridRows = results.data;

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

                        "d1": 0,

                        "total": 0
                    };
                    //   console.log($scope.dayGridRows);
                    $scope.dayGridRows.push(eachRow);
                }

                console.log("Complete add grid rows");
                console.log($scope.dayGridRows);
            });



        }



        // add row for day,day,month 
        $scope.adddayRow = function (index) {
            var eachRow =
            {
                "projectname": "A",
                "projectid": 1,
                "clientname": "test",
                "clientid": 1,
                "tasktypeid": 1,
                "tasktype": "Coding",
                "d1": 0,
                "total": 0,
                startdate: new Date()
            };

            //  console.log(eachRow);
            $scope.dayGridRows.push(eachRow);
        }


        //  day bottom total
        $scope.d1Total = function () {
            //console.log("total day col");
            var total = 0;
            angular.forEach($scope.dayGridRows, function (row, index) {
                total += parseInt(row.d1);
            });
            return total;
        }

        //day all total
        $scope.dayAllTotal = function () {
            return $scope.d1Total();
        }
        // for row and column total
        $scope.getdayRowTotal = function (row) {
            //console.log("call Rolw");
            //console.log(row.d1);

            //   console.log(parseInt(row.d1) + parseInt(row.d2) + parseInt(row.d3) + parseInt(row.d4) + parseInt(row.d5) + parseInt(row.d6) + parseInt(row.d7));
            return parseInt(row.d1);
        }




        // function for current day
        $scope.thisday = function () {
            console.log("Current day");
            $scope.currentDate = new Date();
            $scope.currentDay = $scope.currentDate;
            console.log($scope.currentDay);
            var dd = moment($scope.currentDay.toString());
            console.log(dd);
            $scope.textstring = "";
            $scope.d1 = dd.format("ddd D MMM");
            //  $scope.textstring = dd.format("DD") + " - ";

            $scope.textstring += dd.format("D MMM YYYY");
            $scope.createdayRows($scope.currentDay);

        }

        //function for forward day,day,month
        $scope.ff = function () {
            console.log("Click forwardday");
            $scope.dayGridRows = [];

            var lastdate = moment($scope.currentDay);
            console.log("Old Monday before increment");
            console.log(lastdate);
            var dd = lastdate.add("days", +1);
            var yy = new Date(dd.toDate().toString());
            $scope.currentDay = yy;
            console.log("Current Monday beofre adding rows" + $scope.currentDay);


            console.log(dd.format("DD MMM YYYY"));
            // frwd = dd.format("DD MMM YYYY");
            $scope.textstring = "";
            $scope.d1 = dd.format("ddd D MMM");
            $scope.textstring = dd.format("DD MMM YYYY");

            //$scope.textstring += dd.format("D MMM YYYY");
            $scope.createdayRows($scope.currentDay);
            console.log("Current Monday after adding rows" + $scope.currentDay);

        }


        $scope.previousday = function () {
            $scope.dayGridRows = [];
            console.log("Click Previousday");
            //  console.log($scope.selectedFilterValue);
            var lastdate = moment($scope.currentDay);
            var dd = lastdate.add("days", -1);
            var yy = new Date(dd.toDate().toString());
            $scope.currentDay = yy;
            $scope.currentDate = dd.toDate();
            console.log(dd.format("DD MMM YYYY"));

            console.log("Current Date in previous day" + $scope.currentDay);

            $scope.textstring = "";
            $scope.d1 = dd.format("ddd D MMM");
            $scope.textstring = dd.format("DD MMM YYYY");

            //$scope.textstring += dd.format("D MMM YYYY");
            $scope.createdayRows($scope.currentDay);

        }
        $scope.thisday();




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


