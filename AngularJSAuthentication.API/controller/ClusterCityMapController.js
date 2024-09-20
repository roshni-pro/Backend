'use strict';
app.controller("ClusterCityMapController", ["$scope", '$http',   "CityService",'$routeParams',
    function ($scope, $http,   CityService,  $routeParams) {
        $scope.CityID = $routeParams.id;
        $scope.citys = [];

        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) { });

        function GetMapCityWise() {


            var url = serviceBase + 'api/cluster/GetClusterCityWise?cityid=' + $scope.CityID;
            $http.get(url)
                .success(function (data) {

                    $scope.Peopledatas = data;
                    $scope.initMaps();
                });
        }

       
        var Clustersdata = [];
        var infoWindowNew;        
        $scope.initMaps = function () {

            var clat = $scope.Peopledatas.city.CityLatitude;
            var clong = $scope.Peopledatas.city.CityLongitude;
            var centre = { lat: clat, lng: clong };

            var map1 = new google.maps.Map(document.getElementById("map_canvas"), {
                zoom: 14,
                center: centre,
                mapTypeId: 'terrain'
            });
            var largeInfowindow = new google.maps.InfoWindow();
            var warehouse = [];


            var wurl = serviceBase + 'api/cluster/GetwarehouseLatLong?cityid=' + $scope.CityID;
            $http.get(wurl)
                .success(function (data) {

                    $scope.warehouse = data;
                    for (var i = 0; i < $scope.warehouse.length; i++) {
                        if ($scope.warehouse[i].latitude != 0 && $scope.warehouse[i].longitude != 0) {
                            var marker = new google.maps.Marker({

                                map: map1,
                                //draggable: true,
                                //animation: google.maps.Animation.DROP,
                                position: new google.maps.LatLng($scope.warehouse[i].latitude, $scope.warehouse[i].longitude),
                                title: $scope.warehouse[i].WarehouseName,
                                content: "<div><b>" + $scope.warehouse[i].WarehouseName + "</b><br/>" + ($scope.warehouse[i].Address == "0" ? "" : $scope.warehouse[i].Address) + " </div>",
                                //icon: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'
                                icon: '/Content/warehouse.png'
                            });
                        }


                        marker.addListener('click', function () {
                            populateInfoWindow(this, largeInfowindow);
                        });


                    }

                });




            var cust = [];
            var custmarkers = [];
            var url = serviceBase + 'api/cluster/GetCustomerLatLong?cityid=' + $scope.CityID;
            $http.get(url)
                .success(function (data) {

                    $scope.CustData = data;

                    for (var i = 0; i < $scope.CustData.length; i++) {
                        var custmarker
                        if ($scope.CustData[i].Active == true) {
                            custmarker = new google.maps.Marker({

                                map: map1,
                                draggable: false,
                                //animation: google.maps.Animation.DROP,
                                position: new google.maps.LatLng($scope.CustData[i].lat, $scope.CustData[i].lg),
                                title: $scope.CustData[i].ShopName,
                                content: "<div><b>" + $scope.CustData[i].Skcode + "</b><br/>" + ($scope.CustData[i].ShopName == "0" ? "" : $scope.CustData[i].ShopName) + "<br/>" + $scope.CustData[i].ShippingAddress + " </div>",
                                //icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
                                icon: '/Content/green.png'
                            });

                        }

                        else {
                            custmarker = new google.maps.Marker({
                                map: map1,
                                draggable: false,
                                //animation: google.maps.Animation.DROP,
                                position: new google.maps.LatLng($scope.CustData[i].lat, $scope.CustData[i].lg),
                                title: $scope.CustData[i].ShopName,
                                content: "<div><b>" + $scope.CustData[i].Skcode + "</b><br/>" + ($scope.CustData[i].ShopName == "0" ? "" : $scope.CustData[i].ShopName) + "<br/>" + $scope.CustData[i].ShippingAddress + " </div>",
                                //icon: 'http://maps.google.com/mapfiles/ms/micons/red.png'
                                icon: '/Content/red.png'

                            });
                        }

                        custmarkers.push(custmarker);
                        custmarker.addListener('click', function () {
                            populateInfoWindow(this, largeInfowindow);
                        });

                    }

                });



            var a = '22.79815'//document.getElementById('site-name1').value;
            var b = '75.90214'//document.getElementById('site-name2').value;


            var myArray1 = [];

            function getRandomColor() {
                var letters = '0123456789ABCDEF';
                var color = '#';
                for (var i = 0; i < 6; i++) {
                    color += letters[Math.floor(Math.random() * 16)];
                }
                return color;
            }

            var dta = [];
            for (var s = 0; s < $scope.Peopledatas.clusters.length; s++) {
                createPolygon($scope.Peopledatas.clusters[s]);

            }

            function createPolygon(cluster) {
                var bermudaTriangle = new google.maps.Polygon({
                    paths: cluster.clusterlatlng,
                    strokeColor: getRandomColor(),
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: getRandomColor(),
                    fillOpacity: 0.35,
                    content: cluster.ClusterName,
                    id: cluster.ClusterId,
                    editable: true,

                });
                bermudaTriangle.setMap(map1);
                addListenersOnPolygon(bermudaTriangle);
                bermudaTriangle.addListener('click', function (event) {
                    showClusterName(event, bermudaTriangle);
                });
                infoWindowNew = new google.maps.InfoWindow;
            }


            function showClusterName(event, bermudaTriangle) {
                var clstname = 'Cluster Name: ' + bermudaTriangle.content;
                infoWindowNew.setContent(clstname);
                infoWindowNew.setPosition(event.latLng);
                infoWindowNew.open(map1);
            }

            var infoWindow = new google.maps.InfoWindow;


            function addListenersOnPolygon(bermudaTriangle) {

                bermudaTriangle.getPaths().forEach(function (path, index) {

                    google.maps.event.addListener(path, 'set_at', function (index, obj) {
                        var clusterid = bermudaTriangle.id;
                        var bounds = path.getArray();
                        var latLng = new Array(bounds.length);
                        for (var i = 0; i < bounds.length; i++) {
                            var lat = bounds[i].lat();
                            var lng = bounds[i].lng();
                            var newlatlng = [lat, lng];
                            latLng[i] = newlatlng;
                        }

                        var isadded = false;
                        if (Clustersdata.length > 0) {
                            for (var i = 0; i < Clustersdata.length; i++) {
                                if (Clustersdata[i].ClusterId == clusterid) {
                                    Clustersdata[i].polygon = latLng;
                                    isadded = true;
                                }
                            }
                        }

                        if (!isadded) {
                            Clustersdata.push({
                                ClusterId: clusterid,
                                polygon: latLng
                            });
                        }
                    });

                    google.maps.event.addListener(path, 'insert_at', function (index, obj) {
                        var clusterid = bermudaTriangle.id;
                        var bounds = path.getArray();
                        var latLng = new Array(bounds.length);
                        for (var i = 0; i < bounds.length; i++) {
                            var lat = bounds[i].lat();
                            var lng = bounds[i].lng();
                            var newlatlng = [lat, lng];
                            latLng[i] = newlatlng;
                        }

                        var isadded = false;
                        if (Clustersdata.length > 0) {
                            for (var i = 0; i < Clustersdata.length; i++) {
                                if (Clustersdata[i].ClusterId == clusterid) {
                                    Clustersdata[i].polygon = latLng;
                                    isadded = true;
                                }
                            }
                        }

                        if (!isadded) {
                            Clustersdata.push({
                                ClusterId: clusterid,
                                polygon: latLng
                            });
                        }
                    });
                });
            }


            function populateInfoWindow(marker, infowindow) {
                if (infowindow.marker != marker) {
                    // Clear the infowindow content to give the streetview time to load.
                    infowindow.setContent('');
                    infowindow.marker = marker;
                    // Make sure the marker property is cleared if the infowindow is closed.
                    infowindow.addListener('closeclick', function () {
                        infowindow.marker = null;
                    });
                    infowindow.setContent('<div>' + marker.content + '</div>');
                    infowindow.open(map1, marker);
                }
            }


            // Create the search box and link it to the UI element.
            var input = document.getElementById('pac-input');
            var searchBox = new google.maps.places.SearchBox(input);
            map1.controls[google.maps.ControlPosition.TOP_LEFT].push(input);


            var input1 = document.getElementById('pacskcode-input');
            var searchBox1 = new google.maps.places.SearchBox(input);
            map1.controls[google.maps.ControlPosition.TOP_LEFT].push(input1);

            //// Bias the SearchBox results towards current map's viewport.
            //map1.addListener('bounds_changed', function () {
            //    searchBox.setBounds(map1.getBounds());
            //});

            $(input1).on('keyup', function (e) {
                if (e.keyCode === 13) {
                    var notfound = true;
                    if ($(this).val()) {
                        for (var i = 0; i < $scope.CustData.length; i++) {
                            custmarkers[i].setAnimation(null);
                            if ($scope.CustData[i].Skcode.toLowerCase() == $(this).val().toLowerCase()) {
                                map1.setCenter({ lat: $scope.CustData[i].lat, lng: $scope.CustData[i].lg });
                                custmarkers[i].setAnimation(google.maps.Animation.BOUNCE);
                                notfound = false;
                            }
                        }
                        if (notfound)
                            alert("SKCode Not Found!.");
                    }
                }
            });



            var markers = [];
            // Listen for the event fired when the user selects a prediction and retrieve
            // more details for that place.
            searchBox.addListener('places_changed', function () {
                var places = searchBox.getPlaces();

                if (places.length == 0) {
                    return;
                }

                // Clear out the old markers.
                markers.forEach(function (marker) {
                    marker.setMap(null);
                });
                markers = [];

                // For each place, get the icon, name and location.
                var bounds = new google.maps.LatLngBounds();
                places.forEach(function (place) {
                    if (!place.geometry) {
                        console.log("Returned place contains no geometry");
                        return;
                    }
                    var icon = {
                        url: place.icon,
                        size: new google.maps.Size(71, 71),
                        origin: new google.maps.Point(0, 0),
                        anchor: new google.maps.Point(17, 34),
                        scaledSize: new google.maps.Size(25, 25)
                    };

                    // Create a marker for each place.
                    markers.push(new google.maps.Marker({
                        map: map1,
                        icon: icon,
                        title: place.name,
                        position: place.geometry.location
                    }));

                    if (place.geometry.viewport) {
                        // Only geocodes have viewport.
                        bounds.union(place.geometry.viewport);
                    } else {
                        bounds.extend(place.geometry.location);
                    }
                });
                map1.fitBounds(bounds);
            });
        }


        $scope.savepolygon = function () {
            var url = serviceBase + "api/cluster/updatepolygonList";

            var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            if (UserRole.rolenames.indexOf('HQ Growth Lead') == -1 && UserRole.rolenames.indexOf('HQ Master login') == -1 ) {
                alert("You don't have permission to edit cluster polygon.");
                return false;
            }

            if (Clustersdata != null && Clustersdata.length > 0) {
                var ischange = false;

                for (var i = 0; i < Clustersdata.length; i++) {
                    if (Clustersdata[i].polygon.length > 0) {
                        ischange = true;
                    }
                }

                if (ischange) {
                    $http.post(url, Clustersdata)
                        .success(function (data) {
                            if (data) {
                                alert("Cluster Update successfully.");
                            }
                            else {
                                alert("Some error occurred during update cluster.");
                            }                            
                        })
                        .error(function (data) {
                            alert("Some error occurred during update cluster.");                           
                        })
                }
                else {
                    alert("You are not modify any cluster.");
                }
            }
            else {
                alert("You are not modify any cluster.");
            }
        }

        $scope.back = function () {
            location.href = '#/cluster';
        }

        GetMapCityWise();
    }])