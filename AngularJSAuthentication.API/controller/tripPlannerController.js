
(function () {

    'use strict';

    angular
        .module('app')
        .controller('TripPlannerController', TripPlannerController);
    TripPlannerController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', "TripPlannerService"];

    function TripPlannerController($scope, $filter, $http, ngTableParams, $modal, TripPlannerService) {
        $scope.trips = null;
        $scope.ids = [];
        $scope.clusterId = 46;
        $scope.mapAdditionalOptionList = [];
        $scope.isTestModel = false;
        $scope.allResult = {};
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            "dateLimit": {
                "month": 12
            },
            timePicker: false,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });

        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");


        });


        function initialize(clusterId, startdate, enddate) {
            TripPlannerService.getTrips(clusterId, startdate, enddate, $scope.isTestModel).then(function (x) {
                $scope.trips = x.data;
                console.log('x isss: ', x.data);
                var index = 0;
                $scope.trips.forEach(function (t) {
                    var id = "" + index + "";
                    t.id = id;
                    $scope.ids.push = id;
                    index++;

                    setTimeout(function () {
                        $scope.createMap(t);
                    }, 1000);

                });
                $scope.getAllResult(); 
              
            });
        }



        
        $scope.createMap = function (trip) {

            var wayPionts = [];
            var index = 1;
            for (var i = 0; i < trip.VechielRouteList.length-1; i++) {
                wayPionts.push({
                    location: trip.VechielRouteList[i].InputModel.CLat + " , " + trip.VechielRouteList[i].InputModel.CLng,
                    stopover: true
                });
            }


            var directionsService = new google.maps.DirectionsService();
            //const directionsRenderer = new google.maps.DirectionsRenderer();
            var map = new google.maps.Map(document.getElementById(trip.id), {
                zoom: 12,
                center: { lat: trip.VechielRouteList[0].InputModel.CLat, lng: trip.VechielRouteList[0].InputModel.CLng }
            });

            var directionsDisplay = new google.maps.DirectionsRenderer({
                map: map,
                preserveViewport: true,
                // polylineOptions: polylineOptionsActual
            });

            var request = {
                waypoints: wayPionts,
                optimizeWaypoints: false,
                origin: trip.VechielRouteList[trip.VechielRouteList.length - 1].InputModel.CLat + " , " + trip.VechielRouteList[trip.VechielRouteList.length - 1].InputModel.CLng,
                destination: trip.VechielRouteList[trip.VechielRouteList.length - 1].InputModel.CLat + " , " + trip.VechielRouteList[trip.VechielRouteList.length - 1].InputModel.CLng,
                travelMode: google.maps.TravelMode.DRIVING
            };


            directionsService.route(request, function (response, status) {
               
                if (status === google.maps.DirectionsStatus.OK) {
                    directionsDisplay.setDirections(response);

                }
            });

        }

        $scope.search = function () {

            var f = $('input[name=daterangepicker_start]').val();
            var g = $('input[name=daterangepicker_end]').val();


            initialize($scope.clusterId, f, g);
        };


        $scope.getCode = function (index) {
            return String.fromCharCode(65 + index);
        }


        $scope.getTotalCapacity = function (list) {
            var result = 0;
            list.forEach(function (x) {
                result += (x.Capacity > 0 ? x.Capacity: 0);
            });
            return result;
        }

        $scope.getTotalDistance = function (list) {
            var result = 0;
            list.forEach(function (x) {
                result += x.Distance;
            });
            return result;
        }

        $scope.getTotalTime = function (list) {
            var result = 0;
            list.forEach(function (x) {
                result += x.TimeInMins;
            });
            return result;
        }

        $scope.getAllResult = function () {
            $scope.allResult = {
                time: 0,
                distance: 0,
                capacity:0
            };
            $scope.trips.forEach(x => {
                $scope.allResult.time = $scope.allResult.time + x.TotalTimeInMins;
                $scope.allResult.distance = $scope.allResult.distance + x.TotalDistance;
                $scope.allResult.capacity = $scope.allResult.capacity + (x.TotalCapacity > 0 ? x.TotalCapacity : 0);
            });
        }
    }


})();
