

(function () {
    'use strict';

    angular
        .module('app')
        .controller('GrowthDashboardController', GrowthDashboardController);

    GrowthDashboardController.$inject = ['$scope', '$http', '$modal', "$filter", "GrowthDashboardService", 'WarehouseService'];

    function GrowthDashboardController($scope, $http, $modal, $filter, GrowthDashboardService, WarehouseService) {

        $scope.vm = {};
        $scope.vm.warehouseID = 0;
        $scope.vm.warehouseList = [];


        $scope.initialize = function () {

            WarehouseService.getwarehousewokpp().then(function (result) {
                $scope.vm.warehouseList = result.data;
                $scope.vm.warehouseID = $scope.vm.warehouseList[0].WarehouseId;
                $scope.upateWarehouseList();
                $scope.getAllData();
            });
        };

        $scope.getAllData = function () {
            $scope.vm.totalCompletePercent = 0;
            $scope.vm.infraPercent = 0;
            $scope.vm.peoplePercent = 0;
            $scope.vm.cityMappingPercent = 0;
            $scope.vm.prodPartnersPercent = 0;
            $scope.vm.kpp = { Required: 0, Filled: 0 };
            $scope.vm.agent = { Required: 0, Filled: 0 };
            $scope.vm.brand = { Required: 0, Filled: 0 };
            $scope.vm.retailer = { Required: 0, Filled: 0 };
            $scope.vm.organization = { Required: 0, Filled: 0 };

            GrowthDashboardService.getTotalCompletePercentage($scope.vm.warehouseID).then(function (result) {
                if (result.data && result.data.HubLaunchPercent && result.data.HubLaunchPercent[0] && result.data.HubLaunchPercent[0]) {
                    $scope.vm.totalCompletePercent = result.data.HubLaunchPercent[0].TotalCompletePercent;
                    $scope.vm.infraPercent = result.data.HubLaunchPercent[0].InfraPercent;
                    $scope.vm.peoplePercent = result.data.HubLaunchPercent[0].PeoplePercent;
                    $scope.vm.cityMappingPercent = result.data.HubLaunchPercent[0].CityMappingPercent;
                    $scope.vm.prodPartnersPercent = result.data.HubLaunchPercent[0].ProdPartnersPercent;
                    $scope.renderInfraChart();
                    $scope.renderCityMappingChart();
                    $scope.renderPeopleChartContainer();
                    $scope.renderProductPartnerChart();
                } else {
                    $scope.renderInfraChart();
                    $scope.renderCityMappingChart();
                    $scope.renderPeopleChartContainer();
                    $scope.renderProductPartnerChart();
                }
            });

            GrowthDashboardService.getAgentKPPBrandData($scope.vm.warehouseID).then(function (result) {
                if (result.data && result.data.ProductPartners && result.data.ProductPartners.length > 0) {
                    $scope.vm.agent = result.data.ProductPartners.filter(function (elem) {
                        return elem.Name === 'Agents';
                    });

                    if ($scope.vm.agent && $scope.vm.agent.length > 0) {
                        $scope.vm.agent = $scope.vm.agent[0];
                        $scope.vm.agent = { Required: $scope.vm.agent.RequiredQuantity, Filled: $scope.vm.agent.FilledQuantity };
                    }


                    $scope.vm.kpp = result.data.ProductPartners.filter(function (elem) {
                        return elem.Name === 'KPP';
                    });

                    if ($scope.vm.kpp && $scope.vm.kpp.length > 0) {
                        $scope.vm.kpp = $scope.vm.kpp[0];
                        $scope.vm.kpp = { Required: $scope.vm.kpp.RequiredQuantity, Filled: $scope.vm.kpp.FilledQuantity };
                    }

                    $scope.vm.brand = result.data.ProductPartners.filter(function (elem) {
                        return elem.Name === 'Brand';
                    });

                    if ($scope.vm.brand && $scope.vm.brand.length > 0) {
                        $scope.vm.brand = $scope.vm.brand[0];
                        $scope.vm.brand = { Required: $scope.vm.brand.RequiredQuantity, Filled: $scope.vm.brand.FilledQuantity };
                    }



                }
            });

            GrowthDashboardService.getRetailersData($scope.vm.warehouseID).then(function (result) {
                console.log('result.data : ', result.data);
                if (result.data && result.data.CityMapping && result.data.CityMapping.length > 0) {
                    $scope.vm.retailer = result.data.CityMapping.filter(function (elem) {
                        return elem.Name === 'Retailer Signups';
                    });

                    if ($scope.vm.retailer && $scope.vm.retailer.length > 0) {
                        $scope.vm.retailer = $scope.vm.retailer[0];
                        $scope.vm.retailer = { Required: $scope.vm.retailer.RequiredQuantity, Filled: $scope.vm.retailer.FilledQuantity };
                    }
                }
            });

            GrowthDashboardService.getOrganizationData($scope.vm.warehouseID).then(function (result) {
                console.log('Organizationdata: ', result.data);
                if (result.data && (result.data.RequiredQuantity || result.data.FilledQuantity)) {
                    $scope.vm.organization = { Required: result.data.RequiredQuantity, Filled: result.data.FilledQuantity };
                }

            });
        };


        $scope.renderInfraChart = function () {
            var completeInfra = $scope.vm.infraPercent;
            var remainingInfra = 100 - $scope.vm.infraPercent;
            var chart = new CanvasJS.Chart("infraChartContainer", {
                animationEnabled: true,
                title: {
                    text: "Infrastructure"
                },
                data: [{
                    type: "pie",
                    startAngle: 270,
                    yValueFormatString: "##\"%\"",
                    indexLabel: "{label} {y}",
                    dataPoints: [
                        { y: completeInfra, label: "Complete" },
                        { y: remainingInfra, label: "Remaining" }
                    ]
                }]
            });

            chart.render();

        };

        $scope.renderPeopleChartContainer = function () {
            var completePeople = $scope.vm.peoplePercent;
            var remainingPeople = 100 - $scope.vm.peoplePercent;
            var peopleChart = new CanvasJS.Chart("peopleChartContainer", {
                animationEnabled: true,
                title: {
                    text: "People"
                },
                data: [{
                    type: "pie",
                    startAngle: 270,
                    yValueFormatString: "##\"%\"",
                    indexLabel: "{label} {y}",
                    dataPoints: [
                        { y: completePeople, label: "Complete" },
                        { y: remainingPeople, label: "Remaining" }
                    ]
                }]
            });

            peopleChart.render();

        };

        $scope.renderCityMappingChart = function () {
            var cityMappingComplete = $scope.vm.cityMappingPercent;
            var cityMappingPeopleRemaining = 100 - $scope.vm.cityMappingPercent;
            var cityMappingChart = new CanvasJS.Chart("cityMappingChartContainer", {
                animationEnabled: true,
                title: {
                    text: "City Mapping"
                },
                data: [{
                    type: "pie",
                    startAngle: 270,
                    yValueFormatString: "##\"%\"",
                    indexLabel: "{label} {y}",
                    dataPoints: [
                        { y: cityMappingComplete, label: "Complete" },
                        { y: cityMappingPeopleRemaining, label: "Remaining" }
                    ]
                }]
            });

            cityMappingChart.render();

        };

        $scope.renderProductPartnerChart = function () {
            var productPartnerComplete = $scope.vm.prodPartnersPercent;
            var productPartnerRemaining = 100 - $scope.vm.prodPartnersPercent;
            var productPartnerChart = new CanvasJS.Chart("productPartnerChartContainer", {
                animationEnabled: true,
                title: {
                    text: "Product & Partner"
                },
                data: [{
                    type: "pie",
                    startAngle: 270,
                    yValueFormatString: "##\"%\"",
                    indexLabel: "{label} {y}",
                    dataPoints: [
                        { y: productPartnerComplete, label: "Complete" },
                        { y: productPartnerRemaining, label: "Remaining" }
                    ]
                }]
            });

            productPartnerChart.render();

        };

        $scope.upateWarehouseList = function () {
            if ($scope.vm.warehouseList && $scope.vm.warehouseList.length > 0) {
                $scope.vm.warehouseList.forEach(function (item) {
                    item.combinedName = item.WarehouseName + '    ' + item.CityName;
                });
            }
        }

    }
})();

// api/GrowthModule/GetHubLaunchProgress

//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .controller('GrowthDashboardService', GrowthDashboardService);

//    GrowthDashboardService.$inject = ['$scope'];

//    function GrowthDashboardService($http, ngAuthSettings) {
//        var service = {};
//        var serviceBase = ngAuthSettings.apiServiceBaseUri;

//        service.getTotalCompletePercentage = function (warehouseID) {
//            return $http.get(serviceBase + 'api/GrowthModule/GetHubLaunchProgress?warehouseid=' + warehouseID);
//        };

//        service.getAgentKPPBrandData = function (warehouseID) {
//            return $http.get(serviceBase + 'api/GrowthModule/GetProductPartners?warehouseid=' + warehouseID);
//        };

//        service.getRetailersData = function (warehouseID) {
//            return $http.get(serviceBase + 'api/GrowthModule/GetCityMapping?warehouseid=' + warehouseID);
//        };

//        service.getOrganizationData = function (warehouseID) {
//            return $http.get(serviceBase + 'api/GrowthModule/GetOrganizationData?warehouseid=' + warehouseID);
//        };


//        return service;
//    }
//})();


(function () {
    'use strict';

    angular
        .module('app')
        .factory('GrowthDashboardService', GrowthDashboardService);

    GrowthDashboardService.$inject = ['$http','ngAuthSettings'];

    function GrowthDashboardService($http, ngAuthSettings) {
        var service = {};
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        service.getTotalCompletePercentage = function (warehouseID) {
            return $http.get(serviceBase + 'api/GrowthModule/GetHubLaunchProgress?warehouseid=' + warehouseID);
        };

        service.getAgentKPPBrandData = function (warehouseID) {
            return $http.get(serviceBase + 'api/GrowthModule/GetProductPartners?warehouseid=' + warehouseID);
        };

        service.getRetailersData = function (warehouseID) {
            return $http.get(serviceBase + 'api/GrowthModule/GetCityMapping?warehouseid=' + warehouseID);
        };

        service.getOrganizationData = function (warehouseID) {
            return $http.get(serviceBase + 'api/GrowthModule/GetOrganizationData?warehouseid=' + warehouseID);
        };


        return service;
    }
})();