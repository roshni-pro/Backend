
(function () {
    'use strict';

    angular
        .module('app')
        .controller('DeliveryController', DeliveryController);

    DeliveryController.$inject = ['$scope', "DeliveryService", "OrderMasterService", "$filter", "$http", "ngTableParams", '$modal', '$window'];

    function DeliveryController($scope, DeliveryService, OrderMasterService, $filter, $http, ngTableParams, $modal, $window) {
        $scope.DeliveryIssuanceData = [];//temp
        $scope.totalproducts = false;
        $scope.chkdb = true;
        $scope.oldpords = false;
        $scope.totalpercent = 0;
        $scope.totalproductspace = 0;
        $scope.totalAmountofallproducts = 0;
        $scope.Gtraveltime = 0;
        $scope.Gtraveldistance = 0;
        $scope.IdealTime = 0;
        $scope.VehicleSpace = 0;
        $scope.VehicleSpeed = 20;
        $scope.UnloadingTime = 2;
        $scope.loadingTime = 1.5;
        $scope.finalizebtn = false;

        function getmydistancetime() {
            mynodess();
            if (nodes.length < 2) {
                if (prevNodes.length >= 2) {
                    nodes = prevNodes;
                } else {
                    alert('Less than two destinetions');
                    //return;
                }
            }

            if (directionsDisplay != null) {
                directionsDisplay.setMap(null);
                directionsDisplay = null;
            }

            getDurations(function () {
                // Get config and create initial GA population
                ga.getConfig();
                var pop = new ga.population();
                pop.initialize(nodes.length);
                var route = pop.getFittest().chromosome;

                ga.evolvePopulation(pop, function (update) {
                    $scope.$apply(function () {
                        $scope.Gtraveltime = ((update.population.getFittest().getDistance() / 60)).toFixed(2);
                        $scope.Gtraveldistance = (update.population.getFittest().getDistance() / 100).toFixed(2);
                    });
                    var route = update.population.getFittest().chromosome;
                    var routeCoordinates = [];
                    for (var index in route) {
                        routeCoordinates[index] = nodes[route[index]];
                    }
                    routeCoordinates[route.length] = nodes[route[0]];

                    // Display temp. route
                    if (polylinePath != undefined) {
                        polylinePath.setMap(null);
                    }
                    polylinePath = new google.maps.Polyline({
                        path: routeCoordinates,
                        strokeColor: "#0066ff",
                        strokeOpacity: 0.75,
                        strokeWeight: 2,
                    });
                    polylinePath.setMap(map);
                }, function (result) {
                    // Get route
                    route = result.population.getFittest().chromosome;

                    // Add route to map
                    directionsService = new google.maps.DirectionsService();
                    directionsDisplay = new google.maps.DirectionsRenderer();
                    directionsDisplay.setMap(map);
                    var waypts = [];
                    for (var i = 1; i < route.length; i++) {
                        waypts.push({
                            location: nodes[route[i]],
                            stopover: true
                        });
                    }

                    // Add final route to map
                    var request = {
                        origin: nodes[route[0]],
                        destination: nodes[route[0]],
                        waypoints: waypts,
                        travelMode: google.maps.TravelMode.DRIVING,
                        avoidHighways: false,
                        avoidTolls: false
                    };
                    directionsService.route(request, function (response, status) {
                        if (status == google.maps.DirectionsStatus.OK) {
                            directionsDisplay.setDirections(response);
                        }
                        //clearMapMarkers();
                    });
                });
            });
        }
        function getDurations(callback) {
            var service = new google.maps.DistanceMatrixService();
            service.getDistanceMatrix({
                origins: nodes,
                destinations: nodes,
                travelMode: google.maps.TravelMode.DRIVING,
                avoidHighways: false,
                avoidTolls: false,
            }, function (distanceData) {
                // Create duration data array
                var nodeDistanceData;
                for (var originNodeIndex in distanceData.rows) {
                    nodeDistanceData = distanceData.rows[originNodeIndex].elements;
                    durations[originNodeIndex] = [];
                    for (var destinationNodeIndex in nodeDistanceData) {
                        if (durations[originNodeIndex][destinationNodeIndex] = nodeDistanceData[destinationNodeIndex].duration === undefined) {
                            alert('Error: couldn\'t get a trip duration from API');
                            return;
                        }
                        durations[originNodeIndex][destinationNodeIndex] = nodeDistanceData[destinationNodeIndex].duration.value;
                    }
                }

                if (callback != undefined) {
                    callback();
                }
            });
        }
        function mynodess() {
            var posO = new google.maps.LatLng(22.7196, 75.8677);
            nodes.push(posO);
            for (var m = 0; m < $scope.assignedordersAddress.length; m++) {
                if ($scope.assignedordersAddress[m][0] != 0) {
                    var pos = new google.maps.LatLng($scope.assignedordersAddress[m][0], $scope.assignedordersAddress[m][1]);
                    nodes.push(pos);
                }
            }
            //var posO = new google.maps.LatLng(22.7196, 75.8677);
            //nodes.push(posO);
            //var pos = new google.maps.LatLng(22.687189, 75.833788);
            //nodes.push(pos);
            //pos = new google.maps.LatLng(22.687189, 75.83379);
            //nodes.push(pos);
            //pos = new google.maps.LatLng(22.687189, 75.833794);
            //nodes.push(pos);
            //pos = new google.maps.LatLng(22.684807, 75.8230081);
            //nodes.push(pos);
            //pos = new google.maps.LatLng(22.7196, 75.8677);
            //nodes.push(pos);
        }
        function printDivThroughTrick(divName) {

            var printContents = document.getElementById(divName).innerHTML;
            var originalContents = document.body.innerHTML;

            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                var popupWin = window.open('', '_blank', 'width=800,height=600,scrollbars=no,menubar=no,toolbar=no,location=no,status=no,titlebar=no');
                popupWin.window.focus();
                popupWin.document.write('<!DOCTYPE html><html><head>' +
                    '<link rel="stylesheet" type="text/css" href="style.css" />' +
                    '</head><body onload="window.print()"><div class="reward-body">' + printContents + '</div></html>');
                popupWin.onbeforeunload = function (event) {
                    popupWin.close();
                    return '.\n';
                };
                popupWin.onabort = function (event) {
                    popupWin.document.close();
                    popupWin.close();
                }
            } else {
                var popupWin1 = window.open('', '_blank', 'width=800,height=600');//instead of popupWin used popupWin1
                popupWin1.document.open();
                popupWin1.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + printContents + '</html>');
                popupWin1.document.close();
            }
          ////  popupWin1.document.close();

            return true;
        }
        function printElement(elem) {
            var domClone = elem.cloneNode(true);

            $printSection = document.getElementById("printSection");

            if (!$printSection) {
                var $printSection = document.createElement("div");
                $printSection.id = "printSection";
                document.body.appendChild($printSection);
            }

            $printSection.innerHTML = "";
            $printSection.appendChild(domClone);

        }


        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {
            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A',
                });
            });


            $scope.warehouse = [];
            OrderMasterService.getwarehouse().then(function (results) {

                $scope.warehouse = results.data;
            }, function (error) {
            });



            $scope.open = function (items) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myDboyModal.html",
                        controller: "Dboyctrl", resolve: { obj: function () { return items } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    },
                        function () {
                        })
            };
            $scope.DBoyorders = [];
            $scope.DBoys = [];
            DeliveryService.getdboys().then(function (results) {
                $scope.DBoys = results.data;
                $scope.AddTrack("View", "DeliveryBoy:", "name");
            }, function (error) {
            });
            $scope.deliveryBoy = {};
            $scope.getdborders = function (DB) {

                $scope.DBoyordersNew = [];
                $scope.DBoyordersRedipatch = [];
                $scope.deliveryBoy = JSON.parse(DB);
                if (DB != "") {
                    $scope.chkdb = false;
                    DeliveryService.getordersbyId($scope.deliveryBoy.Mobile).then(function (results) {

                        $scope.DBoyorders = results.data;
                        angular.forEach($scope.DBoyorders, function (value, key) {
                            if (value.ReDispatchCount == 0) {
                                $scope.DBoyordersNew.push(value);
                            }
                            else {
                                $scope.DBoyordersRedipatch.push(value);
                            }
                            console.log($scope.DBoyordersNew);
                            console.log($scope.DBoyordersRedipatch);
                        }, function (error) {
                        });
                        $scope.assignedorders = [];
                        $scope.assignedordersAddress = [];
                        $scope.allproducts = [];
                        $scope.finalizebtn = false;
                        $scope.selectedAll = false;
                    })
                    $scope.VehicleSpace = $scope.deliveryBoy.VehicleCapacity;
                }
            }

            $scope.checkAll = function () {

                if ($scope.selectedAll) {
                    $scope.selectedAll = false;
                } else {
                    $scope.selectedAll = true;
                }
                angular.forEach($scope.DBoyorders, function (trade) {
                    trade.check = $scope.selectedAll;
                });

            };
            $scope.assignedordersAddress = [];
            $scope.oldorders = [];
            $scope.getoldorders = function () {
                var start = "";
                var end = "";
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                if (!$('#dat').val()) {
                    end = '';
                    start = '';
                    alert("Select Start and End Date")
                    return;
                }
                else {
                    start = f.val();
                    end = g.val();
                }
                //var url = serviceBase + "api/DeliveryIssuance?all&id=" + $scope.deliveryBoy.PeopleID;
                var url = serviceBase + "api/DeliveryIssuance?id=" + $scope.deliveryBoy.PeopleID + "&start=" + start + "&end=" + end;
                $http.get(url)
                    .success(function (data) {
                        $scope.oldorders = data;
                        $scope.oldpords = true;
                    })
                    .error(function (data) {
                    })
            }

            $scope.selectedorders = [];
            $scope.assignorders = function () {
                //$scope.allproducts = [];

                $scope.Gtraveltime = 0;
                $scope.Gtraveldistance = 0;
                $scope.IdealTime = 0;

                $scope.assignedorders = [];
                $scope.assignedordersAddress = [];
                $scope.allproducts = [];
                $scope.allproductsfirst = [];
                for (var i = 0; i < $scope.DBoyorders.length; i++) {
                    if ($scope.DBoyorders[i].check == true) {
                        $scope.assignedorders.push($scope.DBoyorders[i]);
                        var cord = [];
                        var l1 = $scope.DBoyorders[i].lat;
                        cord.push(l1);
                        var l2 = $scope.DBoyorders[i].lg;
                        cord.push(l2);
                        $scope.assignedordersAddress.push(cord);
                    }
                }
                nodes = [];
                if ($scope.assignedorders.length > 0) {
                    $scope.selectedorders = angular.copy($scope.assignedorders);
                    var firstreq = true;
                    for (var k = 0; k < $scope.selectedorders.length; k++) {
                        for (var j = 0; j < $scope.selectedorders[k].orderDetails.length; j++) {
                            if (firstreq) {
                                var OD = $scope.selectedorders[k].orderDetails[j];
                                OD.OrderQty = ($scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty).toString();
                                //$scope.allproductsfirst.push($scope.selectedorders[k].orderDetails[j]);
                                $scope.allproductsfirst.push(OD);
                                firstreq = false;
                            } else {
                                var checkprod = true;
                                _.map($scope.allproductsfirst, function (prod) {
                                    // if ($scope.selectedorders[k].orderDetails[j].itemNumber == prod.itemNumber) {//Number beofre mrp
                                    if ($scope.selectedorders[k].orderDetails[j].ItemMultiMRPId == prod.ItemMultiMRPId) {//Number
                                        prod.OrderQty += ", " + $scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty;
                                        prod.qty = $scope.selectedorders[k].orderDetails[j].qty + prod.qty;
                                        prod.TotalAmt = $scope.selectedorders[k].orderDetails[j].TotalAmt + prod.TotalAmt;
                                        checkprod = false;
                                    }
                                })
                                if (checkprod) {
                                    // $scope.allproductsfirst.push($scope.selectedorders[k].orderDetails[j]);
                                    var OD1 = $scope.selectedorders[k].orderDetails[j];//instead of OD used OD1
                                    OD1.OrderQty = ($scope.selectedorders[k].orderDetails[j].OrderId + " - " + $scope.selectedorders[k].orderDetails[j].qty).toString();
                                    //$scope.allproductsfirst.push($scope.selectedorders[k].orderDetails[j]);
                                    $scope.allproductsfirst.push(OD1);
                                }
                            }
                        }
                    }
                    for (var a = 0; a < $scope.allproductsfirst.length; a++) {
                        if ($scope.allproductsfirst[a].qty > 0) {
                            $scope.allproducts.push($scope.allproductsfirst[a]);
                        }
                    }
                    $scope.totalproducts = true;
                    $scope.finalizebtn = true;
                    $scope.finalizeorder();

                } else {
                    alert("Assign Orders");
                }
            }


            $scope.IdealDistTime = function () {
                getmydistancetime();
                $scope.IdealTime = 0;
                if ($scope.loadingTime != 0 && $scope.loadingTime != 0 && $scope.loadingTime != 0) {
                    setTimeout(function () {
                        $scope.IdealTime = (($scope.totalAmountofallproducts * $scope.loadingTime) / 1000 + (($scope.Gtraveldistance / $scope.VehicleSpeed) * 60) + ($scope.assignedorders.length * $scope.UnloadingTime)).toFixed(2);
                        alert("Now can finalize Order");
                    }, 3000);
                } else {
                    alert("Put Loading/Unloading and vehicle speed !");
                }


            }

            $scope.finalizeorder = function () {
                //$scope.$apply(function () {
                $scope.totalproductspace = 0;
                $scope.totalAmountofallproducts = 0;
                for (var i = 0; i < $scope.allproducts.length; i++) {
                    $scope.totalAmountofallproducts = $scope.totalAmountofallproducts + $scope.allproducts[i].TotalAmt;
                    $scope.totalproductspace = $scope.totalproductspace + $scope.allproducts[i].qty * $scope.allproducts[i].SizePerUnit;
                }
                if ($scope.totalproductspace > $scope.VehicleSpace) {
                    //alert("Overweight");
                }
                $scope.totalpercent = Math.round(($scope.totalproductspace / $scope.deliveryBoy.VehicleCapacity) * 100);

                // });        
            }
            $scope.issuance = function () {

                if ($scope.IdealTime == 0) {
                    alert("let the Ideal Time be calculated first ");
                    return;
                }
                if ($scope.allproducts.length > 0) {
                    $("#DIssDe").prop("disabled", true);
                    var url = serviceBase + "api/DeliveryIssuance";
                    var dataToPost = {
                        Cityid: $scope.deliveryBoy.Cityid,
                        city: $scope.deliveryBoy.city,
                        DisplayName: $scope.deliveryBoy.DisplayName,
                        PeopleID: $scope.deliveryBoy.PeopleID,
                        VehicleId: $scope.deliveryBoy.VehicleId,
                        VehicleNumber: $scope.deliveryBoy.VehicleNumber,
                        details: $scope.allproducts,
                        AssignedOrders: $scope.selectedorders,
                        IdealTime: $scope.IdealTime,
                        TravelDistance: $scope.Gtraveldistance
                    };
                    console.log(dataToPost);

                    $http.post(url, dataToPost)
                        .success(function (data) {
                            if (data.DeliveryIssuanceId > 0) {

                                // set DeliveryIssuanceId
                                $scope.DeliveryIssuanceData = data;
                                $scope.finalizebtn = false;
                                $("#assignordersHide").prop("disabled", true);
                                alert("Please print/save  of Assignment IssueId : " + data.DeliveryIssuanceId);
                                //$scope.PrintData();
                                //location.reload();
                            }
                            else {
                                alert("There is Some Problem");
                                //location.reload();
                            }
                        })
                        .error(function (data) {
                            $scope.finalizebtn = false;
                        })
                } else {
                    alert("Select Orders");
                }
            }
            $scope.Acceptissuance = function () {
                var url = serviceBase + "api/DeliveryIssuance";
                var dataToPost = {
                    //Acceptance:true,DeliveryIssuanceId:4,
                    Cityid: $scope.deliveryBoy.Cityid,
                    city: $scope.deliveryBoy.city,
                    DisplayName: $scope.deliveryBoy.DisplayName,
                    PeopleID: $scope.deliveryBoy.PeopleID,
                    VehicleId: $scope.deliveryBoy.VehicleId,
                    VehicleNumber: $scope.deliveryBoy.VehicleNumber,
                    //details: $scope.allproducts
                };
                $http.put(url, dataToPost)
                    .success(function (data) {
                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                    })
                    .error(function (data) {
                    })
            }

            $scope.prodetails = function (items) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myDboyModal1.html",
                        controller: "Dboyctrl1", resolve: { obj: function () { return items } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    },
                        function () {
                        })
            }
            var map;
            var directionsDisplay = null;
            var directionsService;
            var polylinePath;

            var nodes = [];
            var prevNodes = [];
            var markers = [];
            var durations = [];


            // GA code
            var ga = {
                // Default config
                "crossoverRate": 0.5,
                "mutationRate": 0.1,
                "populationSize": 50,
                "tournamentSize": 5,
                "elitism": true,
                "maxGenerations": 50,

                "tickerSpeed": 60,

                // Loads config from HTML inputs
                "getConfig": function () {
                    ga.crossoverRate = parseFloat($('#crossover-rate').val());
                    ga.mutationRate = parseFloat($('#mutation-rate').val());
                    ga.populationSize = parseInt($('#population-size').val()) || 50;
                    ga.elitism = parseInt($('#elitism').val()) || false;
                    ga.maxGenerations = parseInt($('#maxGenerations').val()) || 50;
                },

                // Evolves given population
                "evolvePopulation": function (population, generationCallBack, completeCallBack) {
                    // Start evolution
                    var generation = 1;
                    var evolveInterval = setInterval(function () {
                        if (generationCallBack != undefined) {
                            generationCallBack({
                                population: population,
                                generation: generation,
                            });
                        }

                        // Evolve population
                        population = population.crossover();
                        population.mutate();
                        generation++;

                        // If max generations passed
                        if (generation > ga.maxGenerations) {
                            // Stop looping
                            clearInterval(evolveInterval);

                            if (completeCallBack != undefined) {
                                completeCallBack({
                                    population: population,
                                    generation: generation,
                                });
                            }
                        }
                    }, ga.tickerSpeed);
                },

                // Population class
                "population": function () {
                    // Holds individuals of population
                    this.individuals = [];

                    // Initial population of random individuals with given chromosome length
                    this.initialize = function (chromosomeLength) {
                        this.individuals = [];

                        for (var i = 0; i < ga.populationSize; i++) {
                            var newIndividual = new ga.individual(chromosomeLength);
                            newIndividual.initialize();
                            this.individuals.push(newIndividual);
                        }
                    };

                    // Mutates current population
                    this.mutate = function () {
                        var fittestIndex = this.getFittestIndex();

                        for (var index in this.individuals) {
                            // Don't mutate if this is the elite individual and elitism is enabled 
                            if (ga.elitism != true || index != fittestIndex) {
                                this.individuals[index].mutate();
                            }
                        }
                    };

                    // Applies crossover to current population and returns population of offspring
                    this.crossover = function () {
                        // Create offspring population
                        var newPopulation = new ga.population();

                        // Find fittest individual
                        var fittestIndex = this.getFittestIndex();

                        for (var index in this.individuals) {
                            // Add unchanged into next generation if this is the elite individual and elitism is enabled
                            if (ga.elitism == true && index == fittestIndex) {
                                // Replicate individual
                                var eliteIndividual = new ga.individual(this.individuals[index].chromosomeLength);
                                eliteIndividual.setChromosome(this.individuals[index].chromosome.slice());
                                newPopulation.addIndividual(eliteIndividual);
                            } else {
                                // Select mate
                                var parent = this.tournamentSelection();
                                // Apply crossover
                                this.individuals[index].crossover(parent, newPopulation);
                            }
                        }

                        return newPopulation;
                    };

                    // Adds an individual to current population
                    this.addIndividual = function (individual) {
                        this.individuals.push(individual);
                    };

                    // Selects an individual with tournament selection
                    this.tournamentSelection = function () {
                        // Randomly order population
                        for (var i = 0; i < this.individuals.length; i++) {
                            var randomIndex = Math.floor(Math.random() * this.individuals.length);
                            var tempIndividual = this.individuals[randomIndex];
                            this.individuals[randomIndex] = this.individuals[i];
                            this.individuals[i] = tempIndividual;
                        }

                        // Create tournament population and add individuals
                        var tournamentPopulation = new ga.population();
                        for (var v = 0; v < ga.tournamentSize; v++) { //instead of i used v
                            tournamentPopulation.addIndividual(this.individuals[v]);
                        }

                        return tournamentPopulation.getFittest();
                    };

                    // Return the fittest individual's population index
                    this.getFittestIndex = function () {
                        var fittestIndex = 0;

                        // Loop over population looking for fittest
                        for (var i = 1; i < this.individuals.length; i++) {
                            if (this.individuals[i].calcFitness() > this.individuals[fittestIndex].calcFitness()) {
                                fittestIndex = i;
                            }
                        }

                        return fittestIndex;
                    };

                    // Return fittest individual
                    this.getFittest = function () {
                        return this.individuals[this.getFittestIndex()];
                    };
                },

                // Individual class
                "individual": function (chromosomeLength) {
                    this.chromosomeLength = chromosomeLength;
                    this.fitness = null;
                    this.chromosome = [];

                    // Initialize random individual
                    this.initialize = function () {
                        this.chromosome = [];

                        // Generate random chromosome
                        for (var i = 0; i < this.chromosomeLength; i++) {
                            this.chromosome.push(i);
                        }
                        for (var u = 0; u < this.chromosomeLength; u++) { //instead of i used u
                            var randomIndex = Math.floor(Math.random() * this.chromosomeLength);
                            var tempNode = this.chromosome[randomIndex];
                            this.chromosome[randomIndex] = this.chromosome[u];
                            this.chromosome[u] = tempNode;
                        }
                    };

                    // Set individual's chromosome
                    this.setChromosome = function (chromosome) {
                        this.chromosome = chromosome;
                    };

                    // Mutate individual
                    this.mutate = function () {
                        this.fitness = null;

                        // Loop over chromosome making random changes
                        for (var index in this.chromosome) {
                            if (ga.mutationRate > Math.random()) {
                                var randomIndex = Math.floor(Math.random() * this.chromosomeLength);
                                var tempNode = this.chromosome[randomIndex];
                                this.chromosome[randomIndex] = this.chromosome[index];
                                this.chromosome[index] = tempNode;
                            }
                        }
                    };

                    // Returns individuals route distance
                    this.getDistance = function () {
                        var totalDistance = 0;

                        for (var index in this.chromosome) {
                            var startNode = this.chromosome[index];
                            var endNode = this.chromosome[0];
                            if ((parseInt(index) + 1) < this.chromosome.length) {
                                endNode = this.chromosome[(parseInt(index) + 1)];
                            }

                            totalDistance += durations[startNode][endNode];
                        }

                     ////   totalDistance += durations[startNode][endNode];

                        return totalDistance;
                    };

                    // Calculates individuals fitness value
                    this.calcFitness = function () {
                        if (this.fitness != null) {
                            return this.fitness;
                        }

                        var totalDistance = this.getDistance();

                        this.fitness = 1 / totalDistance;
                        return this.fitness;
                    };

                    // Applies crossover to current individual and mate, then adds it's offspring to given population
                    this.crossover = function (individual, offspringPopulation) {
                        var offspringChromosome = [];

                        // Add a random amount of this individual's genetic information to offspring
                        var startPos = Math.floor(this.chromosome.length * Math.random());
                        var endPos = Math.floor(this.chromosome.length * Math.random());

                        var i = startPos;
                        while (i != endPos) {
                            offspringChromosome[i] = individual.chromosome[i];
                            i++

                            if (i >= this.chromosome.length) {
                                i = 0;
                            }
                        }

                        // Add any remaining genetic information from individual's mate
                        for (var parentIndex in individual.chromosome) {
                            var node = individual.chromosome[parentIndex];

                            var nodeFound = false;
                            for (var offspringIndex in offspringChromosome) {
                                if (offspringChromosome[offspringIndex] == node) {
                                    nodeFound = true;
                                    break;
                                }
                            }

                            if (nodeFound == false) {
                                for (var offspringIndex1 = 0; offspringIndex1 < individual.chromosome.length; offspringIndex1++) { //instead of offspringIndex used offspringIndex1
                                    if (offspringChromosome[offspringIndex1] == undefined) {
                                        offspringChromosome[offspringIndex1] = node;
                                        break;
                                    }
                                }
                            }
                        }

                        // Add chromosome to offspring and add offspring to population
                        var offspring = new ga.individual(this.chromosomeLength);
                        offspring.setChromosome(offspringChromosome);
                        offspringPopulation.addIndividual(offspring);
                    };
                },
            };



            //    // Trigger function

            //$scope.PrintData = function () {
            //    
            //    if ($scope.DeliveryIssuanceData.DeliveryIssuanceId == 0 || $scope.DeliveryIssuanceData.DeliveryIssuanceId == undefined) {
            //        alert("let the Finalize Orders first Then Print");
            //        return;
            //    }
            //    alert("Please take Print otherwise page will be reload");
            //    $scope.printDivs("divtoPrint");

            //}
            //$scope.printDivs = function (div) {
            //    
            //    var docHead = document.head.outerHTML;
            //    var printContents = document.getElementById(div).outerHTML;
            //    var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            //    var newWin = window.open("", "_blank", winAttr);
            //    var writeDoc = newWin.document;
            //    writeDoc.open();
            //    writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            //    writeDoc.close();
            //    newWin.focus();
            //}



            //Print assignment Function
            var $printSection
            document.getElementById("btnPrint").onclick = function () {
                //printElement(document.getElementById("divtoPrint"));

                //alert($scope.DeliveryIssuanceId);
                if ($scope.DeliveryIssuanceData.DeliveryIssuanceId == 0 || $scope.DeliveryIssuanceData.DeliveryIssuanceId == undefined) {
                    // alert("let the Finalize Orders first Then Print");
                    //return;
                }
                else if ($scope.DeliveryIssuanceData.DeliveryIssuanceId > 0) {
                    alert("Now take Print with Assignment Id & Date");
                }
                else {
                }
                printDivThroughTrick("divtoPrint");

                //var modThis = document.querySelector("#printSection .modifyMe");
                //onRouteChangeOff();
                //window.location.reload();

            }
        }
       
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('Dboyctrl1', Dboyctrl1);

    Dboyctrl1.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj"];

    function Dboyctrl1($scope, $http, ngAuthSettings, $modalInstance, obj) {

        $scope.DBoyData = {};
        $scope.orderdetails = [];
        $scope.Orderids = [];


        if (obj) {
            $scope.DBoyData = obj;
            $scope.orderdetails = $scope.DBoyData.details;
            var ids = $scope.DBoyData.OrderIds;
            var str_array = ids.split(',');
            $scope.Orderids = str_array;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('Dboyctrl', Dboyctrl);

    Dboyctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj"];

    function Dboyctrl($scope, $http, ngAuthSettings, $modalInstance, obj) {
        $scope.showfreeItem = false;
        $scope.DBoyData = {};
        $scope.orderdetails = [];
        $scope.OfferData = [];

        if (obj) {

            $scope.DBoyData = obj;

            $scope.orderdetails = $scope.DBoyData.orderDetails;
            var url = serviceBase + "api/DeliveryOrder/GetOfferOnOrder?OrderId=" + obj.OrderId;
            $http.get(url)
                .success(function (results) {
                    if (results.length != 0) {
                        $scope.showfreeItem = true;
                        $scope.OfferData = results;
                    }
                })
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();

