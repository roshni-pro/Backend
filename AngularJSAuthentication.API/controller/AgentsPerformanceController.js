

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AgentsPerformanceController', AgentsPerformanceController);

    AgentsPerformanceController.$inject = ['$scope', '$http', 'CityService', 'ClusterService'];

    function AgentsPerformanceController($scope, $http, CityService, ClusterService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("AgentsDashboard Controller reached");
        //.................Get Agent method start..................    
        $scope.Mobile = "";
        $scope.Salesagent = function () {

            var url = serviceBase + 'api/Agents';

            $http.get(url).success(function (data) {

                $scope.getAgent = data;
                //$scope.PeopleID = $scope.getAgent.PeopleID;

                console.log("$scope.getAgent", $scope.getAgent);

            });
        }
        $scope.Salesagent();

        $scope.cities = [];

        CityService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) { });

        $scope.Clusters = [];
        $scope.Clustercitybased = function (CityId) {

            ClusterService.getCitycluster(CityId).then(function (results) {
                $scope.getcluster = results.data;
            }, function (error) {
            });
        }

        $scope.executive = [];
        $scope.clusterChange = function (clusterid) {

            $http.get(serviceBase + 'api/ChannelPartner/Executive?clstid=' + clusterid).then(function (results) {

                $scope.executive = results.data;
            })
        }
        $scope.beats = [];

        $scope.beatchange = function (cityid, clusterid, executiveid) {

            $http.get(serviceBase + 'api/ChannelPartner/GetBeats?cityId=' + cityid + '&clusterId=' + clusterid + '&agentCode=' + executiveid).then(function (results) {

                console.log("results", results);
                $scope.beats = results.data;
                $scope.CustomerTotal = results.data.cuscount;
                $scope.Totalsell = results.data.Totalsell;
                $scope.TotalCostInMonthsell = results.data.TotalCostIsnMonthsell;
                $scope.TotalCostTodaysell = results.data.TotalCostTodaysell;
                $scope.TotalCostYDaysell = results.data.TotalCostYDaysell;
                $scope.TotalStatusdelivered = results.data.status;
                $scope.TotalStatusCanceled = results.data.statusor;
                $scope.TotalActive = results.data.TotalActive;
                $scope.TotalInActives = results.data.TotalInActives;
                $scope.TotalActiveCustomerInMonth = results.data.TotalActiveCustomerInMonth;
                $scope.TotalActiveCustomerlMonth = results.data.TotalActiveCustomerlMonth;
                $scope.TotalActiveCustomerToday = results.data.TotalActiveCustomerToday;
                $scope.TotalActiveCustomerYDay = results.data.TotalActiveCustomerYDay;
                $scope.TotalCustomersInMonth = results.data.TotalCustomersInMonth;
                $scope.TotalCustomerslMonth = results.data.TotalCustomerslMonth;
                $scope.TotalCustomersToday = results.data.TotalCustomersToday;
                $scope.TotalCustomersyDay = results.data.TotalCustomersyDay;
                $scope.TotalInActiveCustomerYDay = results.data.TotalInActiveCustomerYDay;
                $scope.TotalInActiveCustomerInMonth = results.data.TotalInActiveCustomerInMonth;
                $scope.TotalInActiveCustomerToday = results.data.TotalInActiveCustomerToday;
                $scope.TotalOrders = results.data.TotalOrders;
                $scope.TotalOrdersInmonth = results.data.TotalOrdersInmonth;
                $scope.TotalOrdersToday = results.data.TotalOrdersToday;
                $scope.TotalOrdersyDay = results.data.TotalOrdersyDay;
                //$scope.agentchange2($scope.Mobile);
                //User Tracking


                //End User Tracking
            })
        }

    }
})();

