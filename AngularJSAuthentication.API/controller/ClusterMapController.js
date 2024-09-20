


    (function () {
      //  'use strict';

        angular
            .module('app')
            .controller('ClusterMapController', ClusterMapController);

        ClusterMapController.$inject = ['$scope', 'ClusterService', "$filter", "$http", "ngTableParams", '$modal', 'CityService'];

        function ClusterMapController($scope, ClusterService, $filter, $http, ngTableParams, $modal, CityService) {
            console.log(" Cluster Map City Wise Controller reached");



            $scope.citys = [];

            CityService.getcitys().then(function (results) {
                $scope.citys = results.data;
            }, function (error) { });








            $scope.ViewMapCityWise = function (Cityid) {


                var url = serviceBase + 'api/cluster/GetMapCityWise?cityid=' + Cityid;
                $http.get(url)
                    .success(function (data) {

                        $scope.Peopledatas = data;
                        $scope.initMaps();
                    });
            }

            //GetMapCityWise();






            $scope.initMaps = function () {

                var clat = $scope.Peopledatas[0].CityLatitude;
                var clong = $scope.Peopledatas[0].CityLongitude;
                var centre = { lat: clat, lng: clong };

                var map1 = new google.maps.Map(document.getElementById("map_canvas"), {
                    zoom: 12,
                    center: centre,
                    mapTypeId: 'terrain'
                });

                var a = '22.79815'//document.getElementById('site-name1').value;
                var b = '75.90214'//document.getElementById('site-name2').value;


                var myArray1 = [];
                var myArray2 = [];
                var myArray3 = [];
                var myArray4 = [];

                var myArray5 = [];
                var myArray6 = [];
                var myArray7 = [];
                var myArray8 = [];
                var myArray9 = [];
                var myArray10 = [];
                var myArray11 = [];
                var myArray12 = [];
                var myArray13 = [];
                var myArray14 = [];
                var myArray15 = [];
                var myArray16 = [];
                var myArray17 = [];
                var myArray18 = [];
                var myArray19 = [];
                var myArray20 = [];
                var myArray21 = [];
                var myArray22 = [];
                var myArray23 = [];
                var myArray24 = [];
                var myArray25 = [];



                var dta = [];
                for (var s = 0; s < $scope.Peopledatas.length; s++) {

                    // dta.push({ "lat": $scope.Peopledata[s].Lat, "lng": $scope.Peopledata[s].Long, "clustername": $scope.Peopledata[s].ClusterName });
                    switch ($scope.Peopledatas[s].polygon) {
                        case "p1":
                            myArray1.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                        case "p2":
                            myArray2.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                        case "p3":
                            myArray3.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                        case "p4":
                            myArray4.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p5":
                            myArray5.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                        case "p6":
                            myArray6.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                        case "p7":
                            myArray7.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                        case "p8":
                            myArray8.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                        case "p9":
                            myArray9.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                        case "p10":
                            myArray10.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p11":
                            myArray11.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p12":
                            myArray12.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p13":
                            myArray13.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p14":
                            myArray14.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p15":
                            myArray15.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p16":
                            myArray16.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p17":
                            myArray17.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p18":
                            myArray18.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p19":
                            myArray19.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p20":
                            myArray20.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p21":
                            myArray21.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p22":
                            myArray22.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p23":
                            myArray23.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p24":
                            myArray24.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;

                        case "p25":
                            myArray25.push({ "lat": $scope.Peopledatas[s].latitude, "lng": $scope.Peopledatas[s].longitude });
                            break;
                    }




                }




                //  Construct the polygon.
                var bermudaTriangle = new google.maps.Polygon({
                    paths: myArray1,
                    strokeColor: '#00FFFF',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#00FFFF',
                    fillOpacity: 0.35,
                    content: "Cluster 1"


                });
                bermudaTriangle.setMap(map1);
                bermudaTriangle.addListener('click', showArrays);

                var bermudaTriangle2 = new google.maps.Polygon({
                    paths: myArray2,
                    strokeColor: '#008080',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#008080',
                    fillOpacity: 0.35,
                    content: 'Cluster 2'


                });
                bermudaTriangle2.setMap(map1);
                bermudaTriangle2.addListener('click', showArrays);

                var bermudaTriangle3 = new google.maps.Polygon({
                    paths: myArray3,
                    strokeColor: '#0000FF',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#0000FF',
                    fillOpacity: 0.35,
                    content: 'Cluster 3'


                });
                bermudaTriangle3.setMap(map1);
                bermudaTriangle3.addListener('click', showArrays);

                var bermudaTriangle4 = new google.maps.Polygon({
                    paths: myArray4,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 4'


                });
                bermudaTriangle4.setMap(map1);
                bermudaTriangle4.addListener('click', showArrays);



                var bermudaTriangle5 = new google.maps.Polygon({
                    paths: myArray5,
                    strokeColor: '#00FFFF',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#00FFFF',
                    fillOpacity: 0.35,
                    content: "Cluster5"


                });
                bermudaTriangle5.setMap(map1);
                bermudaTriangle5.addListener('click', showArrays);

                var bermudaTriangle6 = new google.maps.Polygon({
                    paths: myArray6,
                    strokeColor: '#008080',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#008080',
                    fillOpacity: 0.35,
                    content: 'Cluster 6'


                });
                bermudaTriangle6.setMap(map1);
                bermudaTriangle6.addListener('click', showArrays);

                var bermudaTriangle7 = new google.maps.Polygon({
                    paths: myArray7,
                    strokeColor: '#0000FF',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#0000FF',
                    fillOpacity: 0.35,
                    content: 'Cluster 7'


                });
                bermudaTriangle7.setMap(map1);
                bermudaTriangle7.addListener('click', showArrays);

                var bermudaTriangle8 = new google.maps.Polygon({
                    paths: myArray8,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 8'


                });
                bermudaTriangle8.setMap(map1);
                bermudaTriangle8.addListener('click', showArrays);

                var bermudaTriangle9 = new google.maps.Polygon({
                    paths: myArray9,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 9'


                });
                bermudaTriangle9.setMap(map1);
                bermudaTriangle9.addListener('click', showArrays);

                var bermudaTriangle10 = new google.maps.Polygon({
                    paths: myArray10,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 10'


                });
                bermudaTriangle10.setMap(map1);
                bermudaTriangle10.addListener(window, 'load', showArrays);

                var bermudaTriangle11 = new google.maps.Polygon({
                    paths: myArray11,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 11'


                });
                bermudaTriangle11.setMap(map1);
                bermudaTriangle11.addListener(window, 'load', showArrays);

                var bermudaTriangle12 = new google.maps.Polygon({
                    paths: myArray12,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 12'


                });
                bermudaTriangle12.setMap(map1);
                bermudaTriangle12.addListener(window, 'load', showArrays);

                var bermudaTriangle13 = new google.maps.Polygon({
                    paths: myArray13,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 13'


                });
                bermudaTriangle13.setMap(map1);
                bermudaTriangle13.addListener(window, 'load', showArrays);

                var bermudaTriangle14 = new google.maps.Polygon({
                    paths: myArray14,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 14'


                });
                bermudaTriangle14.setMap(map1);
                bermudaTriangle14.addListener(window, 'load', showArrays);

                var bermudaTriangle15 = new google.maps.Polygon({
                    paths: myArray15,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 15'


                });
                bermudaTriangle15.setMap(map1);
                bermudaTriangle15.addListener(window, 'load', showArrays);

                var bermudaTriangle16 = new google.maps.Polygon({
                    paths: myArray16,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 16'


                });
                bermudaTriangle16.setMap(map1);
                bermudaTriangle16.addListener(window, 'load', showArrays);

                var bermudaTriangle17 = new google.maps.Polygon({
                    paths: myArray17,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 17'


                });
                bermudaTriangle17.setMap(map1);
                bermudaTriangle17.addListener(window, 'load', showArrays);

                var bermudaTriangle18 = new google.maps.Polygon({
                    paths: myArray18,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 18'


                });
                bermudaTriangle18.setMap(map1);
                bermudaTriangle18.addListener(window, 'load', showArrays);

                var bermudaTriangle19 = new google.maps.Polygon({
                    paths: myArray19,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 19'


                });
                bermudaTriangle19.setMap(map1);
                bermudaTriangle19.addListener(window, 'load', showArrays);

                var bermudaTriangle20 = new google.maps.Polygon({
                    paths: myArray20,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 20'


                });
                bermudaTriangle20.setMap(map1);
                bermudaTriangle20.addListener(window, 'load', showArrays);

                var bermudaTriangle21 = new google.maps.Polygon({
                    paths: myArray21,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 21'


                });
                bermudaTriangle21.setMap(map1);
                bermudaTriangle21.addListener(window, 'load', showArrays);

                var bermudaTriangle22 = new google.maps.Polygon({
                    paths: myArray22,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 22'


                });
                bermudaTriangle22.setMap(map1);
                bermudaTriangle22.addListener(window, 'load', showArrays);

                var bermudaTriangle23 = new google.maps.Polygon({
                    paths: myArray23,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 23'


                });
                bermudaTriangle23.setMap(map1);
                bermudaTriangle23.addListener(window, 'load', showArrays);

                var bermudaTriangle24 = new google.maps.Polygon({
                    paths: myArray24,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 24'


                });
                bermudaTriangle24.setMap(map1);
                bermudaTriangle24.addListener(window, 'load', showArrays);

                var bermudaTriangle25 = new google.maps.Polygon({
                    paths: myArray25,
                    strokeColor: '#C0C0C0',
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: '#C0C0C0',
                    fillOpacity: 0.35,
                    content: 'Cluster 25'


                });
                bermudaTriangle25.setMap(map1);
                bermudaTriangle25.addListener(window, 'load', showArrays);



                var infoWindow = new google.maps.InfoWindow();

                function showArrays(event) {
                    var vertices = this.getPath();
                    infoWindow.setContent(this.content);
                    infoWindow.setPosition(event.latLng);
                    infoWindow.open(map1);
                }

            }



        }
    })();