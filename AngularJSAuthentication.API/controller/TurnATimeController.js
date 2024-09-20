

(function () {
    'use strict';

    angular
        .module('app')
        .controller('TurnATimeController', TurnATimeController);

    TurnATimeController.$inject = ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal'];

    function TurnATimeController($scope, WarehouseService, $filter, $http, ngTableParams, $modal) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        

        $scope.compid = $scope.UserRole.compid;


        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {

                $scope.warehouse = results.data;
            }, function (error) {
            });
        };


        //(by neha : 11/09/2019 -date range )
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,

                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'DD/MM/YYYY h:mm A'
            }, function (start, end, label) {
                console.log("A new date selection was made: " + start.format('YYYY/MM/DD') + ' to ' + end.format('YYYY/MM/DD'));
            });

            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
            //$('input[name="date"]').on('apply.daterangepicker', function (ev, picker) {
            //    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            //});
            //$('input[name="date"]').on('cancel.daterangepicker', function (ev, picker) {
            //    $(this).val('');
            //});

        });





        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
        });
        $scope.WarehouseId = "";
        $scope.GetData = function () {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val() && $scope.WarehouseId === "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            $scope.TatData = [];
            $scope.TatDataDemo = [];
            var url = serviceBase + "api/TurnATime?start=" + start + "&end=" + end + "&WarehouseId=" + $scope.WarehouseId;
            $http.get(url).success(function (response) {
                $scope.TatData = response;
                $scope.TatDataDemo = response;

                $scope.RtD = 0;
                $scope.RtDhr = 0;
                $scope.AvgRtDhr = 0;
                $scope.AvgRtDhrst = "";

                $scope.Issued = 0;
                $scope.Issuedhr = 0;
                $scope.AvgIssuedhr = 0;
                $scope.AvgIssuedhrst = "";

                $scope.Delivered = 0;
                $scope.Deliveredhr = 0;
                $scope.AvgDeliveredhr = 0;
                $scope.AvgDeliveredhrst = "";



                for (var i = 0; i < $scope.TatDataDemo.length; i++) {

                    if ($scope.TatDataDemo[i].ReadytoDdiffhours > 0) {
                        $scope.RtDhr = $scope.RtDhr + $scope.TatDataDemo[i].ReadytoDdiffhours;
                        //$scope.RtD++;
                        $scope.AvgRtDhrst = "To Dispatch";
                    }
                    if ($scope.TatDataDemo[i].ReadytoDelivereddiffhours > 0) {
                        $scope.Issuedhr = $scope.Issuedhr + $scope.TatDataDemo[i].ReadytoDelivereddiffhours;
                        //$scope.Issued++;
                        $scope.AvgIssuedhrst = "Issued,Shipped";
                    }

                    if ($scope.TatDataDemo[i].Deliverydiffhours > 0) {
                        $scope.Deliveredhr = $scope.Deliveredhr + $scope.TatDataDemo[i].Deliverydiffhours;
                        //$scope.Delivered++;
                        $scope.AvgDeliveredhrst = "Delivered";
                    }

                }
                try {

                    $scope.AvgRtDhr = $scope.RtDhr / $scope.TatDataDemo.length;
                    if (isNaN($scope.AvgRtDhr)) {
                        $scope.AvgRtDhr = 0;
                    }

                    $scope.AvgIssuedhr = $scope.Issuedhr / $scope.TatDataDemo.length;
                    if (isNaN($scope.AvgIssuedhr)) {
                        $scope.AvgIssuedhr = 0;
                    }

                    $scope.AvgDeliveredhr = $scope.Deliveredhr / $scope.TatDataDemo.length;
                    if (isNaN($scope.AvgDeliveredhr)) {
                        $scope.AvgDeliveredhr = 0;
                    }

                }
                catch (err) {
                    aler(err.message);
                }




                $(function () {
                    var chart = new CanvasJS.Chart("chartContainer111",
                        {
                            title: {
                                text: "Bar Chart with Percent"
                            },
                            data: [
                                {
                                    type: "column",
                                    //indexLabel : "{y}",
                                    toolTipContent: "{y}hr",
                                    dataPoints: [
                                        { label: $scope.AvgRtDhrst, y: $scope.AvgRtDhr },
                                        { label: $scope.AvgIssuedhrst, y: $scope.AvgIssuedhr },
                                        { label: $scope.AvgDeliveredhrst, y: $scope.AvgDeliveredhr }

                                    ]
                                }
                            ]
                        });
                    chart.render();
                });
                $scope.callmethod();
            })
                .error(function (data) {
                });
        };
        $scope.callmethod = function () {
            var init;
            return $scope.stores = $scope.TatData,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",
                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },
                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = "";
                },
                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1;
                },
                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1;
                },
                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);
                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange();
                },
                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
                },
                $scope.numPerPageOpt = [50, 100, 200],
                $scope.numPerPage = $scope.numPerPageOpt[0],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        };



        //............................Exel export Method.....................//

        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        };
        $scope.exportData1 = function () {

            alasql('SELECT OrderId,Status,ReadytoDdiffhours,ReadytoDelivereddiffhours,Deliverydiffhours,Skcode,GrossAmount,SalesPerson,ReDispatchCount,CreatedDate,Deliverydate,UpdatedDate,ReadyToDistpatch_Time,Issued_Time,Shipped_Time,FirstRedispatch_Time,SecondRedispatch_Time,ThirdRedispatch_Time,Delivered_Time INTO XLSX("TatData.xlsx",{headers:true}) FROM ?', [$scope.TatData]);
        };
        //............................Exel export Method.....................//
    //}
    //else {
    //    window.location = "#/404";
    //}
    }
})();



