

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ChannelPartnerController', ChannelPartnerController);

    ChannelPartnerController.$inject = ['$scope', '$http', 'CityService', 'peoplesService', 'customerService'];

    function ChannelPartnerController($scope, $http, CityService, peoplesService, customerService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("ChannelPartner Controller reached");
        //.................Get ChannelPartner method start..................

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });
        function convertDate(inputFormat) {
            //var month = new Array();
            var month = [];
            month[0] = "January";
            month[1] = "February";
            month[2] = "March";
            month[3] = "April";
            month[4] = "May";
            month[5] = "June";
            month[6] = "July";
            month[7] = "August";
            month[8] = "September";
            month[9] = "October";
            month[10] = "November";
            month[11] = "December";
            function pad(s) { return (s < 10) ? '0' + s : s; }
            var d = new Date(inputFormat);
            var n = month[d.getMonth()];
            return n;
        }

        $scope.customerdata = [];
        $scope.getChart = function (type, date, city) {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            var Allbydate = [];
            var avgData = [];

            $http.get(serviceBase + 'api/ChannelPartner/Chart?type=' + type + '&start=' + start + '&end=' + end + '&cityid=' + city).then(function (results) {

                console.log("results", results);
                $scope.customerdata = results.data;


                var options = {
                    animationEnabled: true,
                    //height: 560,
                    title: {
                        text: "Agent Sales"
                    },
                    axisY: {
                        //suffix: "0"
                        tittle: "Sales Person"
                    },
                    toolTip: {
                        shared: true,
                        reversed: true
                    },
                    legend: {
                        reversed: true,
                        verticalAlign: "center",
                        horizontalAlign: "right"
                    },
                    data: $scope.customerdata
                };

                $("#chartContainer").CanvasJSChart(options);

            });
        }





        function toolTipContent(e) {
            var str = "";
            var total = 0;
            var str2, str3;
            for (var i = 0; i < e.entries.length; i++) {
                var str1 = "<span style= \"color:" + e.entries[i].dataSeries.color + "\"> " + e.entries[i].dataSeries.name + "</span>: $<strong>" + e.entries[i].dataPoint.y + "</strong>bn<br/>";
                total = e.entries[i].dataPoint.y + total;
                str = str.concat(str1);
            }
            str2 = "<span style = \"color:DodgerBlue;\"><strong>" + (e.entries[0].dataPoint.x).getFullYear() + "</strong></span><br/>";
            total = Math.round(total * 100) / 100;
            str3 = "<span style = \"color:Tomato\">Total:</span><strong> $" + total + "</strong>bn<br/>";
            return (str2.concat(str)).concat(str3);
        }




        $scope.cities = [];

        CityService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) { });

        $scope.peoples = [];

        peoplesService.getpeoples().then(function (results) {

            $scope.peoples = results.data;
            console.log("peoples");
            console.log($scope.peoples);
        }, function (error) {
        });

        $scope.customers = [];
        customerService.getcustomers().then(function (results) {

            $scope.customers = results.data;

        }, function (error) {
            //alert(error.data.message);
            });


        $scope.zones = [];
        $scope.GetZones = function () {
            var url = serviceBase + 'api/inventory/GetZone';
            $http.get(url)
                .success(function (response) {
                    $scope.zones = response;
                });
        };
        $scope.GetZones();

        $scope.regions = [];
        $scope.GetRegions = function (zone) {
            var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
            $http.get(url)
                .success(function (response) {
                    $scope.regions = response;
                });
        };

        $scope.warehouses = [];
        $scope.GetWarehouses = function (warehouse) {
            var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouses = response;
                });
        };

        //$scope.clusters = [];
        //$scope.GetClusters = function (cluster) {
        //    var url = serviceBase + 'api/inventory/GetCluster?warehouseid=' + cluster;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.clusters = response;
        //        });
        //};



        $scope.peopledata = [];

        $scope.agentchange = function (cityid) {
            
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();


            var type = '';
            if (cityid.Agent == 'kpp') {
                type = 'kpp';
            }
            if (cityid.Agent == 'Agent') {
                type = 'Agent';

            }
            $http.get(serviceBase + 'api/ChannelPartner/GetAgentCustomer?CityId=' + cityid.CityId + '&type=' + type + '&start=' + start + '&end=' + end).then(function (results) {

                console.log("results", results);
                $scope.peopledata = results.data;
                //$scope.CustomerTotal = results.data.cuscount;
                //$scope.Totalsell = results.data.Totalsell;
                //$scope.TotalCostInMonthsell = results.data.TotalCostInMonthsell;
                //$scope.TotalCostTodaysell = results.data.TotalCostTodaysell;
                //$scope.TotalCostYDaysell = results.data.TotalCostYDaysell;
                //$scope.TotalStatusdelivered = results.data.status;
                //$scope.TotalStatusCanceled = results.data.statusor;
                //$scope.TotalActive = results.data.TotalActive;
                //$scope.TotalInActives = results.data.TotalInActives;
                //$scope.TotalActiveCustomerInMonth = results.data.TotalActiveCustomerInMonth;
                //$scope.TotalActiveCustomerlMonth = results.data.TotalActiveCustomerlMonth;
                //$scope.TotalActiveCustomerToday = results.data.TotalActiveCustomerToday;
                //$scope.TotalActiveCustomerYDay = results.data.TotalActiveCustomerYDay;
                //$scope.TotalCustomersInMonth = results.data.TotalCustomersInMonth;
                //$scope.TotalCustomerslMonth = results.data.TotalCustomerslMonth;
                //$scope.TotalCustomersToday = results.data.TotalCustomersToday;
                //$scope.TotalCustomersyDay = results.data.TotalCustomersyDay;
                //$scope.TotalInActiveCustomerYDay = results.data.TotalInActiveCustomerYDay;
                //$scope.TotalInActiveCustomerInMonth = results.data.TotalInActiveCustomerInMonth;
                //$scope.TotalInActiveCustomerToday = results.data.TotalInActiveCustomerToday;
                //$scope.TotalOrders = results.data.TotalOrders;
                //$scope.TotalOrdersInmonth = results.data.TotalOrdersInmonth;
                //$scope.TotalOrdersToday = results.data.TotalOrdersToday;
                //$scope.TotalOrdersyDay = results.data.TotalOrdersyDay;
                //$scope.agentchange2($scope.Mobile);
                //User Tracking
                console.log("Peopledata:" + $scope.peopledata);
                //$scope.AddTrack = function () {

                //    var url = serviceBase + "api/trackuser?action=View&item=AgentsId:" + DeliveryboyData.AgentCode;
                //    $http.post(url).success(function (results) { });
                //}
                //$scope.AddTrack();
                //End User Tracking
            })
        }


        $scope.dataselect = [];
        $http.get(serviceBase + 'api/ChannelPartner/GetKPP').success(function (data) {

            $scope.dataselect = data;

        });

    }
})();

