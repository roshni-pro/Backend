

(function () {
    'use strict';

    angular
        .module('app')
        .controller('monthtimesheetController', monthtimesheetController);

    monthtimesheetController.$inject = ['$scope', 'tasktypesService', '$http', 'ngAuthSettings', 'ClientProjectService'];

    function monthtimesheetController($scope, tasktypesService, $http, ngAuthSettings, ClientProjectService) {

        console.log(" time sheet controller start loading");

        $scope.selected = void 0;
        $scope.states = ["Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Dakota", "North Carolina", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming"];

        $scope.AllClientproject = '';
        //function for get all Project
        ClientProjectService.getClientprojects().then(function (results) {
            console.log(results.data);
            $scope.AllClientproject = _.map(results.data, function (o) {
                console.log(o);
                var a = {};
                a = { "name": o.Id + '-' + o.Name };
                return a;
            });
            console.log($scope.AllClientproject);
        }, function (err) {
            console.log(err);
        });




        $scope.tasktypes = [];
        //This function for get all tasktype
        tasktypesService.gettasktypes().then(function (results) {
            $scope.tasktypes = results.data;

        }, function (error) {
            //alert(error.data.message);
        });


        $scope.currentDate = new Date();
        $scope.currentMonday = getMonday($scope.currentDate);
        console.log($scope.currentMonday);
        var dd = moment($scope.dt);
        console.log(dd);


        $scope.monthGridRows = [];


        $scope.selectedFilterValue = 'Month';

        $scope.sd1 = new Date();
        //$scope.textstring = "";
        //$scope.d1 = dd.format("D MMM");
        //$scope.textstring = dd.format("DD") + " - ";
        //$scope.d2 = dd.add("days", 1).format("D MMM");
        //$scope.d3 = dd.add("days", 1).format("D MMM");
        //$scope.d4 = dd.add("days", 1).format("D MMM");
        //$scope.d5 = dd.add("days", 1).format("D MMM");
        //$scope.d6 = dd.add("days", 1).format("D MMM");
        //$scope.d7 = dd.add("days", 1).format("D MMM");
        //$scope.textstring += dd.format("D MMM YYYY");



        $scope.totalRows = 2;



        $scope.daysInMonth = function (month, year) {
            return new Date(year, month, 0).getDate();
        }
        $scope.createMonthRows = function () {

            var Todayis = new Date();

            console.log("NNNNN");
            console.log(Todayis.getDate());
            console.log(Todayis.getFullYear());
            var getDaysValue = $scope.daysInMonth(Todayis.getDate(), Todayis.getFullYear()); //31


            console.log("J Days month");
            console.log(getDaysValue);


            for (var i = 0; i < $scope.totalRows; i++) {
                console.log("Lood Is Run");
                var createeachmonthRow;
                if (Todayis.getDate() == "1") {

                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": " day today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "2") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "3") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": " day test-not-today" },
                    { "d3": 0, "class": " day today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "4") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": " day today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "5") {
                    console.log(" date is 5");
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "6") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "7") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "8") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "9") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "10") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "11") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "12") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "13") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "14") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "15") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "16") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "17") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "18") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "19") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "20") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "21") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": " day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "22") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "23") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "24") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "25") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "26") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "27") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "28") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "29") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "30") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "31") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day today" }
                    ]
                }

                $scope.monthGridRows.push(createeachmonthRow);

            }











            //for (var i = 0; i < $scope.totalRows; i++) {
            //        var eachRow =
            //                 {
            //                     "project": "A",
            //                     "tasktype": "Coding",
            //                     "d1": 0,
            //                     "d2": 0,
            //                     "d3": 0,
            //                     "d4": 0,
            //                     "d5": 0,
            //                     "d6": 0,
            //                     "d7": 0,
            //                     "d8": 0,
            //                     "d9": 0,
            //                     "d10": 0,
            //                     "d11": 0,
            //                     "d12": 0,
            //                     "d13": 0,
            //                     "d14": 0,
            //                     "d15": 0,
            //                     "d16": 0,
            //                     "d17": 0,
            //                     "d18": 0,
            //                     "d19": 0,
            //                     "d20": 0,
            //                     "d21": 0,
            //                     "d22": 0,
            //                     "d23": 0,
            //                     "d24": 0,
            //                     "d25": 0,
            //                     "d26": 0,
            //                     "d27": 0,
            //                     "d28": 0,
            //                     "d29": 0,
            //                     "d30": 0,
            //                     "d31": 0,
            //                 };

            //}
            console.log("Complete month data month data");
            console.log($scope.monthGridRows);
        }



        $scope.addMothRow = function (index) {

            var eachRow =
            {
                "project": "A",
                "tasktype": "Coding",
                "d1": 0,
                "d2": 0,
                "d3": 0,
                "d4": 0,
                "d5": 0,
                "d6": 0,
                "d7": 0,
                "d8": 0,
                "d9": 0,
                "d10": 0,
                "d11": 0,
                "d12": 0,
                "d13": 0,
                "d14": 0,
                "d15": 0,
                "d16": 0,
                "d17": 0,
                "d18": 0,
                "d19": 0,
                "d20": 0,
                "d21": 0,
                "d22": 0,
                "d23": 0,
                "d24": 0,
                "d25": 0,
                "d26": 0,
                "d27": 0,
                "d28": 0,
                "d29": 0,
                "d30": 0,
                "d31": 0,

            };

            var Todayis = new Date();

            console.log("NNNNN");
            console.log(Todayis.getDate());
            console.log(Todayis.getFullYear());
            var getDaysValue = $scope.daysInMonth(Todayis.getDate(), Todayis.getFullYear()); //31


            console.log("J Days month");
            console.log(getDaysValue);


            for (var i = 1; i <= getDaysValue; i++) {
                console.log("Lood Is Run");
                var createeachmonthRow;
                if (Todayis.getDate() == "1") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": " day today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "2") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "3") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": " day test-not-today" },
                    { "d3": 0, "class": " day today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "4") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": " day today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "5") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "6") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "7") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "8") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "9") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "10") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "11") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "12") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "13") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "14") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "15") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "16") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "17") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "18") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "19") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "20") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "21") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": " day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "22") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "23") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "24") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "25") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "26") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "27") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "28") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "29") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "30") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day today" },
                    { "d31": 0, "class": "day test-not-today" }
                    ]
                }

                if (Todayis.getDate() == "31") {
                    createeachmonthRow = [{ "project": "A", "class": "normal-class" },
                    { "tasktype": "Coding", "class": "normal-class" },
                    { "d1": 0, "class": "day test-not-today" },
                    { "d2": 0, "class": "day test-not-today" },
                    { "d3": 0, "class": "day test-not-today" },
                    { "d4": 0, "class": "day test-not-today" },
                    { "d5": 0, "class": "day test-not-today" },
                    { "d6": 0, "class": "day test-not-today" },
                    { "d7": 0, "class": "day test-not-today" },
                    { "d8": 0, "class": "day test-not-today" },
                    { "d9": 0, "class": "day test-not-today" },
                    { "d10": 0, "class": "day test-not-today" },
                    { "d11": 0, "class": "day test-not-today" },
                    { "d12": 0, "class": "day test-not-today" },
                    { "d13": 0, "class": "day test-not-today" },
                    { "d14": 0, "class": "day test-not-today" },
                    { "d15": 0, "class": "day test-not-today" },
                    { "d16": 0, "class": "day test-not-today" },
                    { "d17": 0, "class": "day test-not-today" },
                    { "d18": 0, "class": "day test-not-today" },
                    { "d19": 0, "class": "day test-not-today" },
                    { "d20": 0, "class": "day test-not-today" },
                    { "d21": 0, "class": "day test-not-today" },
                    { "d22": 0, "class": "day test-not-today" },
                    { "d23": 0, "class": "day test-not-today" },
                    { "d24": 0, "class": "day test-not-today" },
                    { "d25": 0, "class": "day test-not-today" },
                    { "d26": 0, "class": "day test-not-today" },
                    { "d27": 0, "class": "day test-not-today" },
                    { "d28": 0, "class": "day test-not-today" },
                    { "d29": 0, "class": "day test-not-today" },
                    { "d30": 0, "class": "day test-not-today" },
                    { "d31": 0, "class": "day today" }
                    ]
                }

                $scope.monthGridRows.push(createeachmonthRow);

            }



            //     console.log(eachRow);
            ////$scope.monthGridRows.push(createeachmonthRow);
        }


        // remove row for week,day,month

        $scope.removeMonthRow = function (index) {
            $scope.monthGridRows.splice(index, 1);
        }



        // month bottom total
        $scope.md1Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[2].d1);
            });
            console.log(total);
            return total;
        }
        $scope.md2Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[3].d2);
            });
            return total;
        }
        $scope.md3Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[4].d3);
            });
            return total;
        }
        $scope.md4Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[5].d4);
            });
            return total;
        }
        $scope.md5Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[6].d5);
            });
            return total;
        }
        $scope.md6Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[7].d6);
            });
            return total;
        }
        $scope.md7Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[8].d7);
            });
            return total;
        }
        $scope.md8Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[9].d8);
            });
            return total;
        }
        $scope.md9Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[10].d9);
            });
            return total;
        }
        $scope.md10Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[11].d10);
            });
            return total;
        }
        $scope.md11Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[12].d11);
            });
            return total;
        }
        $scope.md12Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[13].d12);
            });
            return total;
        }
        $scope.md13Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[14].d13);
            });
            return total;
        }
        $scope.md14Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[15].d14);
            });
            return total;
        }
        $scope.md15Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[16].d15);
            });
            return total;
        }
        $scope.md16Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[17].d16);
            });
            return total;
        }
        $scope.md17Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[18].d17);
            });
            return total;
        }
        $scope.md18Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[19].d18);
            });
            return total;
        }
        $scope.md19Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[20].d19);
            });
            return total;
        }
        $scope.md20Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[21].d20);
            });
            return total;
        }
        $scope.md21Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[22].d21);
            });
            return total;
        }
        $scope.md22Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[23].d22);
            });
            return total;
        }
        $scope.md23Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[24].d23);
            });
            return total;
        }
        $scope.md24Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[25].d24);
            });
            return total;
        }
        $scope.md25Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[26].d25);
            });
            return total;
        }
        $scope.md26Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[27].d26);
            });
            return total;
        }
        $scope.md27Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[28].d27);
            });
            return total;
        }
        $scope.md28Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[29].d28);
            });
            return total;
        }
        $scope.md29Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[30].d29);
            });
            return total;
        }
        $scope.md30Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[31].d30);
            });
            return total;
        }
        $scope.md31Total = function () {
            var total = 0;
            angular.forEach($scope.monthGridRows, function (row, index) {
                total += parseInt(row[32].d31);
            });
            return total;
        }

        $scope.getMothRowTotal = function (row) {
            // console.log(" get mnth row is coming here ");
            console.log(parseInt(row[2].d1) + parseInt(row[3].d2) + parseInt(row[4].d3) + parseInt(row[5].d4) + parseInt(row[6].d5) + parseInt(row[7].d6) + parseInt(row[8].d7) + parseInt(row[9].d8) + parseInt(row[10].d9) + parseInt(row[11].d10) + parseInt(row[12].d11) + parseInt(row[13].d12) + parseInt(row[14].d13) + parseInt(row[15].d14) + parseInt(row[16].d15) + parseInt(row[17].d16) + parseInt(row[18].d17) + parseInt(row[19].d18) + parseInt(row[20].d19) + parseInt(row[21].d20) + parseInt(row[22].d21) + parseInt(row[23].d22) + parseInt(row[24].d23) + parseInt(row[25].d24) + parseInt(row[26].d25) + parseInt(row[27].d26) + parseInt(row[28].d27) + parseInt(row[29].d28) + parseInt(row[30].d29) + parseInt(row[31].d30) + parseInt(row[32].d31));
            return parseInt(row[2].d1) + parseInt(row[3].d2) + parseInt(row[4].d3) + parseInt(row[5].d4) + parseInt(row[6].d5) + parseInt(row[7].d6) + parseInt(row[8].d7) + parseInt(row[9].d8) + parseInt(row[10].d9) + parseInt(row[11].d10) + parseInt(row[12].d11) + parseInt(row[13].d12) + parseInt(row[14].d13) + parseInt(row[15].d14) + parseInt(row[16].d15) + parseInt(row[17].d16) + parseInt(row[18].d17) + parseInt(row[19].d18) + parseInt(row[20].d19) + parseInt(row[21].d20) + parseInt(row[22].d21) + parseInt(row[23].d22) + parseInt(row[24].d23) + parseInt(row[25].d24) + parseInt(row[26].d25) + parseInt(row[27].d26) + parseInt(row[28].d27) + parseInt(row[29].d28) + parseInt(row[30].d29) + parseInt(row[31].d30) + parseInt(row[32].d31);
        }
        $scope.monthAllTotal = function () {
            // console.log("monthAllTotal");
            //console.log(parseInt(row.d1) + parseInt(row.d2) + parseInt(row.d3) + parseInt(row.d4) + parseInt(row.d5) + parseInt(row.d6) + parseInt(row.d7) + parseInt(row.d8) + parseInt(row.d9) + parseInt(row.d10) + parseInt(row.d11) + parseInt(row.d12) + parseInt(row.d13) + parseInt(row.d14) + parseInt(row.d15) + parseInt(row.d16) + parseInt(row.d17) + parseInt(row.d18) + parseInt(row.d19) + parseInt(row.d20) + parseInt(row.d21) + parseInt(row.d22) + parseInt(row.d23) + parseInt(row.d24) + parseInt(row.d25) + parseInt(row.d26) + parseInt(row.d27) + parseInt(row.d28) + parseInt(row.d29) + parseInt(row.d30) + parseInt(row.d31));
            return $scope.md1Total() + $scope.md2Total() + $scope.md3Total() + $scope.md4Total() + $scope.md5Total() + $scope.md6Total() + $scope.md7Total() + $scope.md8Total() + $scope.md9Total() + $scope.md10Total() + $scope.md11Total() + $scope.md12Total() + $scope.md13Total() + $scope.md14Total() + $scope.md15Total() + $scope.md16Total() + $scope.md17Total() + $scope.md18Total() + $scope.md19Total() + $scope.md20Total() + $scope.md21Total() + $scope.md22Total() + $scope.md23Total() + $scope.md24Total() + $scope.md25Total() + $scope.md26Total() + $scope.md27Total() + $scope.md28Total() + $scope.md29Total() + $scope.md30Total() + $scope.md31Total();
        }

        // function for previous week,day,month

        $scope.previousMonth = function () {

            console.log("Click Previousmonth");
            console.log($scope.selectedFilterValue);
            var lastdate = moment(frwd);
            var dd = lastdate.add("days", -30);
            $scope.currentDate = dd.toDate();
            console.log(dd.format("DD MMM YYYY"));
            frwd = dd.format("DD MMM YYYY");
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
        }


        // function for forward week,day,month

        $scope.forwardMonth = function () {

            console.log("Click forwardmonth");
            console.log($scope.selectedFilterValue);
            var lastdate = moment(frwd);
            var dd = lastdate.add("days", +30);
            $scope.currentDate = dd.toDate();
            console.log(dd.format("DD MMM YYYY"));
            frwd = dd.format("DD MMM YYYY");
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
        }



        // function for current week

        // function for current month
        $scope.thisMonth = function () {
            var d = new Date();
            console.log("month data coming here");
            console.log(d);
            d.setDate(1);
            console.log("First date");
            console.log(d);
            var dd = moment(d.toString());
            console.log(dd);
            $scope.textstring = "";
            $scope.md1 = dd.format("D dd");
            $scope.textstring = dd.format("DD MMM") + " - ";
            $scope.md2 = dd.add("days", 1).format("D dd");
            $scope.md3 = dd.add("days", 1).format("D dd");
            $scope.md4 = dd.add("days", 1).format("D dd");
            $scope.md5 = dd.add("days", 1).format("D dd");
            $scope.md6 = dd.add("days", 1).format("D dd");
            $scope.md7 = dd.add("days", 1).format("D dd");
            $scope.md8 = dd.add("days", 1).format("D dd");
            $scope.md9 = dd.add("days", 1).format("D dd");
            $scope.md10 = dd.add("days", 1).format("D dd");
            $scope.md11 = dd.add("days", 1).format("D dd");
            $scope.md12 = dd.add("days", 1).format("D dd");
            $scope.md13 = dd.add("days", 1).format("D dd");
            $scope.md14 = dd.add("days", 1).format("D dd");
            $scope.md15 = dd.add("days", 1).format("D dd");
            $scope.md16 = dd.add("days", 1).format("D dd");
            $scope.md17 = dd.add("days", 1).format("D dd");
            $scope.md18 = dd.add("days", 1).format("D dd");
            $scope.md19 = dd.add("days", 1).format("D dd");
            $scope.md20 = dd.add("days", 1).format("D dd");
            $scope.md21 = dd.add("days", 1).format("D dd");
            $scope.md22 = dd.add("days", 1).format("D dd");
            $scope.md23 = dd.add("days", 1).format("D dd");
            $scope.md24 = dd.add("days", 1).format("D dd");
            $scope.md25 = dd.add("days", 1).format("D dd");
            $scope.md26 = dd.add("days", 1).format("D dd");
            $scope.md27 = dd.add("days", 1).format("D dd");
            $scope.md28 = dd.add("days", 1).format("D dd");
            $scope.md29 = dd.add("days", 1).format("D dd");
            $scope.md30 = dd.add("days", 1).format("D dd");
            $scope.md31 = dd.add("days", 1).format("D dd");
            $scope.textstring += dd.format("D MMM YYYY");
        }
        // function for current day


        $scope.firstDayOfMonth = function () {
            var d = new Date(Date.apply(null, arguments));
            d.setDate(1);
            console.log("firstDayOfMonth");
            console.log(d.toISOString());
            return d.toISOString();
        }
        $scope.monthString = function (d) {
            var j = moment(m);
            var startmonth = j.format('DD');
            var dd = d.getDate();
            var enddate = d.setDate(d + 6);
            var dd1 = moment(enddate);
            var mm = dd1.format('dd MM YYYY');
            var finalstring = startmonth + ' - ' + mm;
            console.log(finalstring);
        }

        $scope.textstring = function (d) {
            var j = moment(m);
            var startmonth = j.format('DD');
            var dd = d.getDate();
            var enddate = d.setDate(d + 6);
            var dd1 = moment(enddate);
            var mm = dd1.format('dd MM YYYY');
            var finalstring = startmonth + ' - ' + mm;
            console.log(finalstring);
        }



        // function for previous date
        var frwd = $scope.currentDate.toString();

        $scope.getNumber = function () {
            var newArr = [];
            for (var i = 0; i < 5; i++) {
                newArr.push(i);
            }
            return newArr;
        }

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






        $scope.thisMonth(new Date());


        //$scope.getMonday(new Date());


        //for bootstrap calendar
        $scope.open = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.opened = true;
        };

        $scope.dateOptions = {
            formatYear: 'yy',
            startingDay: 1
        };
        $scope.format = 'dd-MMMM-yyyy';


        // calling create day,week,month function

        $scope.createMonthRows();






        // for show and hide week sday month
        $scope.month = true;

        // for table show and hide

        $scope.tableMonthView = true;


        $scope.selectDayCalendar = function () {
            // for table show and hide

            $scope.tableMonthView = true;

            $scope.month = true;

            $scope.selectedFilterValue = "Month";
            console.log("$scope.selectedFilterValue");
            console.log($scope.selectedFilterValue);
        }

        $scope.selectWeekCalendar = function () {

            // for table show and hide
            $scope.tableDayView = false;
            $scope.tableWeekView = true;
            $scope.tableMonthView = false;

            $scope.month = false;
            $scope.day = false;
            $scope.week = true;
            $scope.selectedFilterValue = "Week";
            console.log("$scope.selectedFilterValue");
            console.log($scope.selectedFilterValue);
        }

        $scope.selectMonthCalendar = function () {

            // for table show and hide

            $scope.tableMonthView = true;

            $scope.month = true;

            $scope.selectedFilterValue = "Month";
            console.log("$scope.selectedFilterValue");
            console.log($scope.selectedFilterValue);
        }


    //console.log("controller end loaded");



    }
})();

