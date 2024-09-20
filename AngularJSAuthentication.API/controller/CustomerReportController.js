

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CustomerReportController', CustomerReportController);

    CustomerReportController.$inject = ['$scope'];

    function CustomerReportController($scope) {
        $scope.datrange = '';
        $scope.datefrom = '';
        $scope.dateto = '';
        var bydate = [];
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker24Hour: true,
                format: 'YYYY-MM-DD h:mm A'
            });
        });
        $scope.getFormatedDate = function (date) {
            console.log("here get for")
            date = new Date(date);
            var dd = date.getDate();
            var mm = date.getMonth() + 1; //January is 0!
            var yyyy = date.getFullYear();
            if (dd < 10) {
                dd = '0' + dd;
            }
            if (mm < 10) {
                mm = '0' + mm;
            }
            var date1 = yyyy + '-' + mm + '-' + dd;//instead of date used date1
            return date;
        }
        var Allbydate = [];
        $scope.filter = function () {
            $scope.dataforsearch = { datefrom: "", dateto: "" };
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            if (!$('#dat').val()) {
                $scope.dataforsearch.datefrom = '';
                $scope.dataforsearch.dateto = '';
            }
            else {
                $scope.dataforsearch.datefrom = f.val();
                $scope.dataforsearch.dateto = g.val();
            }

            console.log("here daterange");
            console.log($scope.dataforsearch.datefrom);
            console.log($scope.dataforsearch.dateto);
            $scope.customerdata = [];
            $scope.id = 1;//for static purpose
            var url = serviceBase + 'api/CustomersReport/bydate?datefrom=' + $scope.dataforsearch.datefrom + "&dateto=" + $scope.dataforsearch.dateto + "&id=" + $scope.id;
            $http.get(url)
                .success(function (data) {

                    $scope.customerdata = data;
                    var count = 0;
                    for (var i = 0; i < data.length; i++) {

                        count++;
                    }
                    console.log(count);
                    $scope.getChart();
                });
        }

        //function exist(date) {
        //    var st= false;
        //    var formatted = convertDate(date);
        //    _.map(Allbydate, function (obj) {
        //        if(obj.label==formatted)
        //        {
        //            return true;
        //        }
        //    })
        //    return st;
        //    }

        //for date conversion
        function convertDate(inputFormat) {
            function pad(s) { return (s < 10) ? '0' + s : s; }
            var d = new Date(inputFormat);
            return [pad(d.getDate()), pad(d.getMonth() + 1), d.getFullYear()].join('/');
        }
        $scope.getChart = function () {
            Allbydate = [];
            _.map($scope.customerdata, function (obj) {
                var checkexits = false;
                var formatdate = convertDate(obj.CreatedDate);
                if (Allbydate.length > 0) {
                    _.map(Allbydate, function (col) {
                        if (col.label == formatdate) {
                            col.y = col.y + 1;
                            checkexits = true;
                        }
                    })
                    if (checkexits == false) {
                        var ob = { y: 1, label: formatdate }
                        Allbydate.push(ob);
                    }
                }
                else {
                    var ob1 = { y: 1, label: formatdate }//instead of ob used ob1
                    Allbydate.push(ob1);
                }
            })
            //_.map($scope.customerdata, function (obj) {
            //    var rs = exist(obj.CreatedDate);
            //    var formatdate = convertDate(obj.CreatedDate)
            //    if (rs == true)
            //    {
            //        _.map(Allbydate, function (col) {
            //            if(col.label==formatdate)
            //            {
            //                col.y = col.y + 1;
            //            }
            //        })
            //    }
            //    else
            //    {
            //        var ob = { y: 1, label: formatdate }              
            //            Allbydate.push(ob);        
            //    }               
            //})

            var chart = new CanvasJS.Chart("chartContainer",
                {
                    title: {
                        text: "Customer registration Details"
                    },
                    animationEnabled: true,
                    axisY: {
                        title: "Customer registration"
                    },
                    legend: {
                        verticalAlign: "bottom",
                        horizontalAlign: "center"
                    },
                    theme: "theme2",
                    data: [
                        {
                            type: "column",
                            showInLegend: true,
                            legendMarkerColor: "grey",
                            legendText: "Total Registration",
                            dataPoints: Allbydate
                        }
                    ]
                });
            chart.render();
        }

        // all registered customer data

        $scope.filterall = function () {


            $scope.customerdata = [];
            var url = serviceBase + 'api/CustomersReport/all';
            $http.get(url)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    $scope.customerdata = data;
                    $scope.total = $scope.customerdata.length;
                    var a = $scope.customerdata.length;

                    $(function () {
                        var chart = new CanvasJS.Chart("chartContainerall",
                            {
                                title: {
                                    text: "Total registered customer",
                                    fontFamily: "arial black"
                                },
                                animationEnabled: true,
                                legend: {
                                    verticalAlign: "bottom",
                                    horizontalAlign: "center"
                                },
                                credits: {
                                    enabled: false
                                },
                                theme: "theme1",
                                data: [
                                    {
                                        type: "pie",
                                        indexLabelFontFamily: "Garamond",
                                        indexLabelFontSize: 20,
                                        indexLabelFontWeight: "bold",
                                        startAngle: 0,
                                        indexLabelFontColor: "MistyRose",
                                        indexLabelLineColor: "darkgrey",
                                        indexLabelPlacement: "inside",
                                        toolTipContent: "{name}: {y}",
                                        showInLegend: true,
                                        // indexLabel: "#percent%",
                                        indexLabel: "{y}",
                                        dataPoints: [
                                            { y: a, name: "Total registration", legendMarkerType: "circle" }

                                        ]
                                    }
                                ]
                            });
                        chart.render();
                    });
                });
        }
        $scope.filteractive = function () {

            $scope.customerdata = [];
            var url = serviceBase + 'api/CustomersReport/all';
            $http.get(url)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    $scope.customerdata = data;
                    var a = $scope.customerdata.length;


                    var count = 0;
                    for (var i = 0; i < data.length; i++) {
                        if (data[i].Active == true)
                            count++;
                    }

                    console.log(count);

                    var countv = 0;//instead of counti used countv
                    for (var v = 0; v < data.length; v++) {//instead of i used v
                        if (data[v].Active == false)
                            countv++;
                    }

                    console.log(countv);



                    $(function () {
                        var chart = new CanvasJS.Chart("chartContaineractiveinactive",
                            {
                                title: {
                                    text: "active & inactive customers",
                                    fontFamily: "arial black"
                                },
                                animationEnabled: true,
                                legend: {
                                    verticalAlign: "bottom",
                                    horizontalAlign: "center"
                                },
                                credits: {
                                    enabled: false
                                },
                                theme: "theme1",
                                data: [
                                    {
                                        type: "pie",
                                        indexLabelFontFamily: "Garamond",
                                        indexLabelFontSize: 20,
                                        indexLabelFontWeight: "bold",
                                        startAngle: 0,
                                        indexLabelFontColor: "MistyRose",
                                        indexLabelLineColor: "darkgrey",
                                        indexLabelPlacement: "inside",
                                        toolTipContent: "{name}: {y}",
                                        showInLegend: true,
                                        // indexLabel: "#percent%",
                                        indexLabel: "{y}",
                                        dataPoints: [
                                            { y: count, name: "Active customers", legendMarkerType: "triangle" },
                                            { y: counti, name: "Inactive customers", legendMarkerType: "square" }

                                        ]
                                    }
                                ]
                            });
                        chart.render();
                    });
                });
        }


    // for all orders graph

    }
})();
