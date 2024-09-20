

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AsignDayController', AsignDayController);

    AsignDayController.$inject = ['$scope', "$filter", "$http", "ngTableParams"];

    function AsignDayController($scope, $filter, $http, ngTableParams) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {


            $scope.Salesasign = function () {

                var url = serviceBase + 'api/AsignDay';
                $http.get(url)
                    .success(function (data) {
                        $scope.Peopledata = data;
                    });
            }
            $scope.Salesasign();
            $scope.Day = "";
            $scope.dataselect = [];
            $scope.fulldata = [];
            $scope.selectedDayChanged = function (Day) {

                alert(Day);
                console.log($scope.fulldata);
                if (Day.length > 0) {
                    $scope.dataselect = $scope.fulldata.filter((customer) = customer.Day === Day);
                } else {
                    $scope.dataselect = $scope.fulldata;
                }
            }

            $scope.selectedItemChanged = function (data) {
                console.log(data);

                var url = serviceBase + "api/AsignDay/customer?id=" + data.PeopleID + "&day=" + data.Day;

                $http.get(url)
                    .success(function (data) {
                        if (data.length == 0) {
                            alert("Not Found");
                        }

                        $scope.fulldata = data;
                        $scope.dataselect = data;
                        console.log(data);
                    });
            }



            $scope.submit = function () {
                $scope.salesassignday = []
                for (var i = 0; i < $scope.dataselect.length; i++) {
                    if ($scope.dataselect[i].check == true) {
                        $scope.salesassignday.push($scope.dataselect[i]);
                    }
                }
                $scope.selectedorders = angular.copy($scope.salesassignday);
                var url = serviceBase + "api/AsignDay";
                var dataTopost = {
                    clist: $scope.selectedorders,
                }
                if (dataTopost.clist.length != 0) {
                    $http.put(url, dataTopost)
                        .then(function (response) {
                            $scope.salesPDay = response.data;
                            alert("data updated Successfully");
                            location.reload();
                        });
                }
                else {
                    alert("Please select checkBox");
                }
            }
        }
       
    }
})();