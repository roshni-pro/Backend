


(function () {
    'use strict';

    angular
        .module('app')
        .controller('AgentsDashboardController', AgentsDashboardController);

    AgentsDashboardController.$inject = ['$scope', '$http'];

    function AgentsDashboardController($scope, $http) {
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

        $scope.agentchange = function (DeliveryboyData) {
            $scope.agentcode = $scope.DeliveryboyData.AgentCode;
            $http.get(serviceBase + 'api/Agents/GetAgentCustomer?agentcode=' + $scope.agentcode).then(function (results) {

                console.log("results", results);
                $scope.CustomerTotal = results.data.cuscount;
                $scope.Totalsell = results.data.Totalsell;
                $scope.TotalCostInMonthsell = results.data.TotalCostInMonthsell;
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
                console.log("Tracking Code");
                $scope.AddTrack = function () {

                    var url = serviceBase + "api/trackuser?action=View&item=AgentsId:" + DeliveryboyData.AgentCode;
                    $http.post(url).success(function (results) { });
                }
                $scope.AddTrack();
                //End User Tracking
            })
        }

        //$scope.agentchange2 = function (Mobile) {
        //   
        //    $scope.agentcode = $scope.DeliveryboyData.AgentCode;
        //    $http.get(serviceBase + 'api/Agents/AllOrder?Mobile=' + $scope.Mobile).then(function (results) {
        //      

        //        console.log("results", results);

        //    })

        //}
    }
})();
