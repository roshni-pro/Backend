

(function () {
    'use strict';

    angular
        .module('app')
        .controller('HealthChartController', HealthChartController);

    HealthChartController.$inject = ['$scope', '$http', 'CityService', 'peoplesService', 'customerService'];

    function HealthChartController($scope, $http, CityService, peoplesService, customerService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("ChannelPartner Controller reached");
        //.................Get ChannelPartner method start..................
        $scope.Agentpending = [];
        $scope.Agentpendings = [];


        //Getchart();

        $scope.onload = function () {






            $scope.dataselect = [];
            var url = serviceBase + "api/HealthChart/GetPeoples";
            $http.get(url)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                    }

                    $scope.dataselect = data;
                    console.log(data);
                });
        }
        $scope.onload();

        $scope.mySplit = function (string, nb) {
            var array = string.split(',');
            return array;
        }

        $scope.catType = [
            { value: 1, text: "Category" },
            { value: 2, text: "SubCatogary" },
            { value: 3, text: "SubSubCatogory" },
            { value: 4, text: "item" }
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
        $scope.GetChart = function (agentId) {

            $scope.CategoryCompare = [];
            var myarray = [];
            var myarray1 = [];
            var ids = [];
            _.each($scope.examplemodel, function (o2) {

                console.log(o2);
                for (var i = 0; i < $scope.dataselect.length; i++) {
                    if ($scope.dataselect[i].PeopleID == o2.id) {

                        ids.push(o2.id);
                    }
                }
            });


            var url = serviceBase + 'api/HealthChart/GetChart';

            $scope.check = false;
            $scope.checks = false;
            var agentid = ids;
            console.log(JSON.stringify(agentId));
            $http.post(url, agentid)
                .success(function (response) {
                    $scope.CategoryCompare = response;


                    for (var i = 0; i < $scope.CategoryCompare.length; i++) {

                        myarray.push({ "y": $scope.CategoryCompare[i].CompletePercent, "label": $scope.CategoryCompare[i].dboyNames, "id": $scope.CategoryCompare[i].IncompleteColums, "agent": $scope.CategoryCompare[i].Agent })
                        myarray1.push({ "y": $scope.CategoryCompare[i].IncompletePercent, "label": $scope.CategoryCompare[i].dboyNames, "id": $scope.CategoryCompare[i].IncompleteColums, "agent": $scope.CategoryCompare[i].Agent })

                    }




                    var chart = new CanvasJS.Chart("chartContainer", {
                        animationEnabled: true,
                        theme: "light1", //"light1", "dark1", "dark2"
                        title: {
                            text: "Agent & KPP Health Chart"
                        },
                        axisY: {
                            interval: 10,
                            suffix: "%"
                        },
                        toolTip: {
                            shared: true
                        },

                        data: [{
                            click: function (e) {
                                // alert("dataSeries Event => Type: " + e.dataSeries.type + ", dataPoint { x:" + e.dataPoint.y + ", y: " + e.dataPoint.id + " }");

                                $scope.Agentpending = [];
                                $scope.selectedagent = e.dataPoint.agent;
                                $scope.check = true
                                $scope.checks = false;

                                for (var i = 0; i < $scope.CategoryCompare.length; i++) {
                                    if ($scope.CategoryCompare[i].Agent == e.dataPoint.agent) {
                                        $scope.Agentpending = $scope.CategoryCompare[i].CompletColums;
                                    }

                                }
                            },
                            type: "stackedBar100",
                            toolTipContent: "{label}<br><b>{name}:</b> {y} (#percent%)",
                            color: "Green",
                            showInLegend: true,
                            name: "Completed",
                            dataPoints: myarray
                        },
                        {
                            click: function (e) {
                                // alert("dataSeries Event => Type: " + e.dataSeries.type + ", dataPoint { x:" + e.dataPoint.y + ", y: " + e.dataPoint.id + " }");

                                $scope.Agentpendings = [];
                                $scope.selectedagents = e.dataPoint.agent;
                                $scope.checks = true;
                                $scope.check = false

                                for (var i = 0; i < $scope.CategoryCompare.length; i++) {
                                    if ($scope.CategoryCompare[i].Agent == e.dataPoint.agent) {
                                        $scope.Agentpendings = $scope.CategoryCompare[i].IncompleteColums;
                                    }

                                }
                            },
                            type: "stackedBar100",
                            color: "Red",
                            toolTipContent: "<b>{name}:</b> {y} (#percent%)",
                            showInLegend: true,
                            name: "Incompleted",
                            dataPoints: myarray1
                        }]

                    });
                    chart.render();


                });

        }


    }
})();


