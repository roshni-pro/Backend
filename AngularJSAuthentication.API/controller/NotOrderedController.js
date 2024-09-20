

(function () {
    'use strict';

    angular
        .module('app')
        .controller('NotOrderedController', NotOrderedController);

    NotOrderedController.$inject = ['$scope', '$http', '$modal', "$filter"];

    function NotOrderedController($scope, $http, $modal, $filter) {
        $scope.skcode = "";
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
               // document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });
        var url = serviceBase + 'api/AsignDay';
        $http.get(url)
            .success(function (data) {
                $scope.Peopledata = data;
            });
        $scope.selectedItemChanged = function (data) {
            $scope.status = null;
            //$scope.VisitNotVisitDatabyStatus=undefined;
            console.log(data);
            $scope.Salespersonid = data.PeopleID;
            var url = serviceBase + "api/NotOrdered/VisitNotVisit?salespersonid=" + data.PeopleID;

            $http.get(url)
                .success(function (data) {
                    if (data.length === 0) {
                        alert("Not Found");
                    }

                    $scope.notOrdered = data;
                    $scope.callmethod();
                    //$scope.dataselect = data;
                    //console.log(data);
                });
        }
        $scope.VisitNotVisit = function () {
            if ($scope.Salespersonid !== undefined) {
                var url = serviceBase + "api/NotOrdered/VisitNotVisitStatus?salespersonid=" + $scope.Salespersonid + "&Visited=" + $scope.status;
                $http.get(url)
                    .success(function (data) {
                        if (data.length === 0) {
                            alert("Not Found");
                        }
                        $scope.notOrdered = data;
                        $scope.callmethod();
                        //$scope.dataselect = data;
                        //console.log(data);
                    });
            }
            else {
                alert('Please Select Sales Person')
                $scope.status = null;
            }

        }
        $scope.getNotorderddata = function () {
            $scope.notOrdered = {};
            var url = serviceBase + "api/NotOrdered";
            $http.get(url)
                .success(function (results) {

                    $scope.notOrdered = results;
                    $scope.callmethod();
                })
                .error(function (data) {
                    console.log(data);
                })
        };
        $scope.getNotorderddata();
        $scope.Search = function () {

            if ($('#dat').val() === null || $('#dat').val() === "") {
                $('input[name=daterangepicker_start]').val("");
                $('input[name=daterangepicker_end]').val("");
            }
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();
            var end = g.val();
            if ($scope.skcode !== "" || (start !== null && start !== "")) {
                $scope.notOrdered = {};
                var url = serviceBase + "api/NotOrdered/search?start=" + start + "&end=" + end + "&skcode=" + $scope.skcode + "&salespersonid=" + $scope.data.PeopleID;
                $http.get(url).success(function (response) {
                    $scope.notOrdered = response;
                    $scope.callmethod();
                });
            }
            else {
                alert('Please select one parameter');
            }
        }
        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.callmethod = function () {
            var init;
            // if ($scope.VisitNotVisitData != undefined && $scope.VisitNotVisitDatabyStatus==undefined)
            // {
            //     $scope.stores = $scope.VisitNotVisitData
            // }
            // else if ($scope.VisitNotVisitDatabyStatus != undefined) {
            //     $scope.stores = $scope.VisitNotVisitDatabyStatus
            // }
            //else{
            //     $scope.stores = $scope.notOrdered
            //}
            $scope.stores = $scope.notOrdered;
            $scope.stores;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

           

                $scope.numPerPageOpt = [10, 25, 50, 100];
                $scope.numPerPage = $scope.numPerPageOpt[0];
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

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1;
            }

            $scope.search = function () {
                console.log("search");
                console.log($scope.stores);
                console.log($scope.searchKeywords);

                $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
            }

            $scope.order = function (rowName) {
                console.log("order"); console.log($scope.stores);
                $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
            }
            $scope.exportData = function () {
                alasql('SELECT ShopName,Skcode,SalespersonName,status,Comment, CreatedDate INTO XLSX("NotOrdered.xlsx",{headers:true}) FROM ?', [$scope.currentPageStores]);
            };


            $scope.GetLocationData = function (data) {

                $(function () {
                    var lat = data.lat,
                        lng = data.lg,
                        latlng = new google.maps.LatLng(lat, lng),
                        image = 'http://www.google.com/intl/en_us/mapfiles/ms/micons/blue-dot.png';
                    var mapOptions = {
                        center: new google.maps.LatLng(lat, lng),
                        zoom: 13,
                        mapTypeId: google.maps.MapTypeId.ROADMAP,
                        panControl: true,
                        panControlOptions: {
                            position: google.maps.ControlPosition.TOP_RIGHT
                        },
                        zoomControl: true,
                        zoomControlOptions: {
                            style: google.maps.ZoomControlStyle.LARGE,
                            position: google.maps.ControlPosition.TOP_left
                        }
                    },
                        map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions),
                        marker = new google.maps.Marker({
                            position: latlng,
                            map: map,
                            icon: image
                        });

                    var input = document.getElementById('searchTextField');
                    var autocomplete = new google.maps.places.Autocomplete(input, {
                        types: ["geocode"]
                    });

                    autocomplete.bindTo('bounds', map);
                    var infowindow = new google.maps.InfoWindow();

                    google.maps.event.addListener(autocomplete, 'place_changed', function (event) {
                        infowindow.close();
                        var place = autocomplete.getPlace();
                        if (place.geometry.viewport) {
                            map.fitBounds(place.geometry.viewport);
                        } else {
                            map.setCenter(place.geometry.location);
                            map.setZoom(17);
                        }
                        moveMarker(place.name, place.geometry.location);
                        $('.MapLat').val(place.geometry.location.lat());
                        $('.MapLon').val(place.geometry.location.lng());
                    });
                    google.maps.event.addListener(map, 'click', function (event) {
                        $('.MapLat').val(event.latLng.lat());
                        $('.MapLon').val(event.latLng.lng());
                        infowindow.close();
                        var geocoder = new google.maps.Geocoder();
                        geocoder.geocode({
                            "latLng": event.latLng
                        }, function (results, status) {
                            console.log(results, status);
                            if (status === google.maps.GeocoderStatus.OK) {
                                console.log(results);
                                var lat = results[0].geometry.location.lat(),
                                    lng = results[0].geometry.location.lng(),
                                    placeName = results[0].address_components[0].long_name,
                                    latlng = new google.maps.LatLng(lat, lng);

                                moveMarker(placeName, latlng);
                                $("#searchTextField").val(results[0].formatted_address);
                            }
                        });
                    });

                    function moveMarker(placeName, latlng) {
                        // marker.setIcon(image);
                        // marker.setPosition(latlng);
                        // infowindow.setContent(placeName);
                        //infowindow.open(map, marker);
                    }
                });


            };
        }

        }
    }) ();