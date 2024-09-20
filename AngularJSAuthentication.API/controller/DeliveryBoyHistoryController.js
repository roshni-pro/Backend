

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DeliveryBoyHistoryController', DeliveryBoyHistoryController);

    DeliveryBoyHistoryController.$inject = ['$scope', "$http", "DeliveryService", "ngAuthSettings", '$filter', "ngTableParams", '$modal'];

    function DeliveryBoyHistoryController($scope, $http, DeliveryService, ngAuthSettings, $filter, ngTableParams, $modal) {
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                //locale: {
                format: 'YYYY-MM-DD h:mm A'
                //}

            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
               // document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });





      
        $scope.DBoys = [];
        DeliveryService.getdboys().then(function (results) {
            $scope.DBoys = results.data;
        }, function (error) {
        });

        $scope.DeliveryBoyHistory = function (dId) {
            var s = null;
            var e = null;
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var s1 = f.val();//instead of s used s1
            var e1 = g.val();//instead of e used e1
            var url = serviceBase + "api/GpsController/data?id=" + $scope.dbmob.PeopleID + '&sDate=' + s + '&eDate=' + e;
            $http.get(url)
                .success(function (data) {
                    var customer = data.customer;
                    $scope.gpsdata = [{
                        "gpsId": 0,
                        "CreatedDate": "SK Warehouse",//"2017-01-01T00:00:00.000+00:00",
                        "CustomerName": "Warehouse",
                        "isDestination": false,
                        "lat": 22.704765,
                        "lg": 75.825104,
                        "status": "", "Deleted": false, "StartOrEnd": "start",
                        "count": 0
                    }];
                    if (data.deliveryboy.length > 0) {
                        for (var g = 0; g < data.deliveryboy.length; g++) {
                            if (data.deliveryboy[g].lat == 22.704765 && data.deliveryboy[g].lg == 75.825104) {
                                $scope.gpsdata[0].count = data.deliveryboy[0].count + 1;
                            }
                            else if (data.deliveryboy[g].lat > 0) {
                                $scope.gpsdata.push(data.deliveryboy[g])
                            }
                        }
                    }
                    $(document).ready(function () { initialize(); });

                    function initialize() {
                        var mapOptions = {
                            center: new google.maps.LatLng(22.7048262, 75.8251145),
                            zoom: 14,
                            mapTypeId: google.maps.MapTypeId.ROADMAP
                        };
                        var map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);
                        var infoWindow = new google.maps.InfoWindow();
                        //var lat_lng = new Array();
                        //var lat_lg = new Array();
                        var lat_lng = [];
                        var lat_lg = [];
                        var latlngbounds = new google.maps.LatLngBounds();
                        var title = "";
                        var description = "";
                        for (i = 0; i < customer.length; i++) {
                            var data = customer[i];
                            var myLatlng = new google.maps.LatLng(data.lat, data.lg);
                            lat_lg.push(myLatlng);
                            var marker = new google.maps.Marker({
                                position: myLatlng,
                                map: map,
                                Label: "SK",
                                title: data.CustomerName
                            });
                            latlngbounds.extend(marker.position);
                            (function (marker, data) {
                                google.maps.event.addListener(marker, "click", function (e) {
                                    infoWindow.setContent(data.CustomerName);
                                    infoWindow.open(map, marker);
                                });
                            })(marker, data);
                        }

                        for (i = 0; i < $scope.gpsdata.length; i++) {
                            var data = $scope.gpsdata[i];
                            var label = String.fromCharCode(i + 97);
                            var myLatlng = new google.maps.LatLng(data.lat, data.lg);
                            lat_lng.push(myLatlng);
                            var marker = new google.maps.Marker({
                                position: myLatlng,
                                map: map,
                                Label: label,
                                icon: '../ControllersReports/delboy.png',
                                //icon: {
                                //    path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
                                //    strokeColor: "green",
                                //    scale: 5
                                //},
                                title: data.CustomerName
                            });
                            latlngbounds.extend(marker.position);
                            (function (marker, data) {
                                google.maps.event.addListener(marker, "click", function (e) {
                                    var array = data.CreatedDate.split("+");
                                    infoWindow.setContent(array[0]);
                                    infoWindow.open(map, marker);
                                });
                            })(marker, data);
                        }
                        map.setCenter(latlngbounds.getCenter());
                        map.fitBounds(latlngbounds);

                        //***********ROUTING****************//
                        //Initialize the Path Array
                        var path = new google.maps.MVCArray();
                        //Initialize the Direction Service
                        var service = new google.maps.DirectionsService();
                        //Set the Path Stroke Color
                        var poly = new google.maps.Polyline({ map: map, strokeColor: '#FFAABB' });
                        //Loop and Draw Path Route between the Points on MAP                              

                        for (var i = 0; i < lat_lng.length; i++) {
                            var src = lat_lng[i];
                            if ((i + 1) < lat_lng.length) {
                                var des = lat_lng[i + 1];
                            }
                            else
                                var des1 = lat_lng[i];//instead of des used des1
                            path.push(src);
                            poly.setPath(path);
                            //service.route({
                            //    origin: src,
                            //    destination: des,
                            //    travelMode: google.maps.DirectionsTravelMode.DRIVING
                            //}, function (result, status) {
                            //    if (status == google.maps.DirectionsStatus.OK) {
                            //        for (var i = 0, len = result.routes[0].overview_path.length; i < len; i++) {
                            //            path.push(result.routes[0].overview_path[i]);
                            //        }
                            //    }
                            //});
                        }
                        //for (var i = 0; i < lat_lg.length; i++) {
                        //    path.push(lat_lg[i]);
                        //}
                    }
                    console.log($scope.gpsdata);
                })
                .error(function (data) {

                });
        }
        $scope.dbmob = {};
        $scope.dboyId = function (data) {
            $scope.dbmob = JSON.parse(data);
        }
        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.allOrders;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }

        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
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
        };

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

        $scope.showOrders = function () {
            if ($scope.dbmob.Mobile) {
                $http.get(serviceBase + 'api/DeliveryOrder?M=all&mob=' + $scope.dbmob.Mobile).then(function (results) {
                    $scope.allOrders = results.data;
                    $scope.callmethod();
                });
            } else { alert("Select Delivery Boy") }
        }
        $scope.showDetail = function (data) {
            console.log("Edit Dialog called city");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myredispatchdetail.html",
                    controller: "myredispatchdetailCtrl", resolve: { order: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {


                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };
    }
})();
