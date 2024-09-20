'use strict'
app.controller('DeliveryBoyReportCtrl', ['$scope', "$filter", "$http", "ngTableParams", function ($scope, $filter, $http, ngTableParams) {
    $scope.datrange = '';
    $scope.datefrom = '';
    $scope.type = 1;
    $scope.dateto = '';

    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker24Hour: true,
            format: 'YYYY-MM-DD h:mm A'
        });

        $('.input-group-addon').click(function () {
            $('input[name="daterange"]').trigger("select");
            //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

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
        var date = yyyy + '-' + mm + '-' + dd;
        return date;
    }
    var Allbydate = [];
    var finallistoflist = [];
    var filterData = [];

    var AlllistDPs = [];
    var PenlistDps = [];
    var delilistDPs = [];
    var cncllistDps = [];
    var RedisplistDps = [];

    $scope.filtype = "";
    $scope.dataselect = [];
    $scope.examplemodel = [];
    $scope.exampledata = $scope.dataselect;
    $scope.examplesettings = {
        displayProp: 'DisplayName', idProp: 'PeopleID',
        //externalIdProp: 'Mobile',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $http.get(serviceBase + 'api/DeliveryBoy').then(function (results) {
        if (results.data != "null") {
            $scope.dataselect = results.data;
            console.log($scope.dataselect);
        }
    });

    $scope.selType = [
    { value: 1, text: "Orders" },
    { value: 2, text: "Values" }
    ];

    $scope.genereteReport = function () {
        
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
        var ids = [];
        _.each($scope.examplemodel, function (o2) {
            console.log(o2);
            for (var i = 0; i < $scope.dataselect.length; i++) {
                if ($scope.dataselect[i].PeopleID == o2.id) {
                    var Row =
                     {
                         "id": o2.id, "mob": $scope.dataselect[i].Mobile
                     };
                    ids.push(Row);
                }
            }
        })
        var datatopost = {
            datefrom: $scope.dataforsearch.datefrom,
            dateto: $scope.dataforsearch.dateto,
            ids: ids
        }
        console.log(datatopost);
        $scope.NewExportData = [];
        $scope.datas = {};
        var url = serviceBase + "api/DeliveryBoyReport";
        $http.post(url, datatopost)       
        .success(function (data) {
            
            if (data.length == 0) {
                alert("Not Found");
                $scope.getChartOrder(null);
            }
            else {
       
                Allbydate = data;                
                $scope.datas = data;
                
                for (var i = 0; i < $scope.datas.length; i++) {
                    var tts = {
                        AllOrderAmount: '', Canceled: '', CanceledOderAmount: '', DBoyId: '', Delivered: '', DeliveredOrderAmount: '', Pending: '',
                        PendingOderAmount: '', ReadyToDispatch: '', ReadyToDispatchOderAmount: '', Redispatched: '',
                        RedispatchedOrderAmount: '', name: '', totalOrder: '',
                    }

                    tts.AllOrderAmount = $scope.datas[i].AllOrderAmount;
                    tts.Canceled = $scope.datas[i].Canceled;
                    tts.CanceledOderAmount = $scope.datas[i].CanceledOderAmount;
                    tts.DBoyId = $scope.datas[i].DBoyId;
                    tts.Delivered = $scope.datas[i].Delivered;
                    tts.DeliveredOrderAmount = $scope.datas[i].DeliveredOrderAmount;
                    tts.Pending = $scope.datas[i].Pending;
                    tts.PendingOderAmount = $scope.datas[i].PendingOderAmount;
                    tts.ReadyToDispatch = $scope.datas[i].ReadyToDispatch;
                    tts.ReadyToDispatchOderAmount = $scope.datas[i].ReadyToDispatchOderAmount;
                    tts.Redispatched = $scope.datas[i].Redispatched;
                    tts.RedispatchedOrderAmount = $scope.datas[i].RedispatchedOrderAmount;
                    tts.name = $scope.datas[i].name;
                    tts.totalOrder = $scope.datas[i].totalOrder;
                    $scope.NewExportData.push(tts);
                }
            }
          
            console.log(data);
            //$scope.filtdata();
            $scope.filtdataPie();
          
            if ($scope.type == 1) {
                $scope.filtdata();
            } else {
                $scope.filtdataValue();
            }
        });
    }






    $scope.chartdata = function (gd1, j, naam) {
        if (gd1.length > 0) {
            var orsum = {
                type: "column",
                showInLegend: true,
                lineThickness: 2,
                color: j,
                markerType: "square",
                name: naam,
                indexLabel: "{y}",
                title: "Order/Sale(in 10000)",
                dataPoints: gd1
            }
            return orsum;
        } else {
            return null;
        }
    }

    $scope.Piechart = function (dps, idd, name) {
        
        var pchart = new CanvasJS.Chart("piechartContainer" + idd,
	{
	    theme: "theme2",
	    title: {
	        text: name
	    },
	    data: [
		{
           
		    type: "pie",
		    showInLegend: true,
		    toolTipContent: "{y} orders - #percent %",
		    //yValueFormatString: "#0.#,,. Million",
		    indexLabel: "{y}",
		    legendText: "{indexLabel}",
		    dataPoints: dps
		}
	    ]
	});
        pchart.render();
    }

    $scope.filtdata = function () {
        finallistoflist = [];
        AlllistDPs = [];
        PenlistDps = [];
        delilistDPs = [];
        cncllistDps = [];
        RedisplistDps = [];
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var fd = { y: Allbydate[i].totalOrder, label: Allbydate[i].name };
                if (fd != null) {
                    AlllistDPs.push(fd);
                }
            }
        }
        if (AlllistDPs.length > 0) {
            var gd = $scope.chartdata(AlllistDPs, "#7f6084", "AllOrders");
            finallistoflist.push(gd);
        }
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var cd = { y: Allbydate[i].Pending, label: Allbydate[i].name };
                if (cd != null) {
                    PenlistDps.push(cd);
                }
            }
        }
        if (PenlistDps.length > 0) {
            var gd = $scope.chartdata(PenlistDps, "#ffff99", "Pending");
            finallistoflist.push(gd);
        }
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var dd = { y: Allbydate[i].Delivered, label: Allbydate[i].name };
                if (fd != null) {
                    delilistDPs.push(dd);
                }
            }
        }       
        if (delilistDPs.length > 0) {
            var gd = $scope.chartdata(delilistDPs, "#86b402", "Delivered");
            finallistoflist.push(gd);
        }
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var cd = { y: Allbydate[i].Canceled, label: Allbydate[i].name };
                if (cd != null) {
                    cncllistDps.push(cd);
                }
            }
        }
        if (cncllistDps.length > 0) {
            var gd = $scope.chartdata(cncllistDps, "#c24642", "Canceled");
            finallistoflist.push(gd);
        }
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var rd = { y: Allbydate[i].Redispatched, label: Allbydate[i].name };
                if (rd != null) {
                    RedisplistDps.push(rd);
                }
            }
        }
        if (RedisplistDps.length > 0) {
            var gd = $scope.chartdata(RedisplistDps, "#369ead", "Redispatched");
            finallistoflist.push(gd);
        }
        if (finallistoflist.length > 0) {
            $scope.getChartOrder(finallistoflist, "Orders");
        }
    }

    $scope.filtdataPie = function () {
        
        $("#pierows").empty();
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var e = $('<div class="col-md-4" style="height:300px!important;"></div>');
                var myid = "piechartContainer" + i;
                e.attr('id', myid);
                $('#pierows').append(e);

                var fd = [
                    { y: Allbydate[i].Pending, indexLabel: "Pending" },
                 { y: Allbydate[i].Delivered, indexLabel: "Delivered" },
                 { y: Allbydate[i].Canceled, indexLabel: "Canceled" },
                 { y: Allbydate[i].Redispatched, indexLabel: "Redispatched" }]
                $scope.Piechart(fd, i, Allbydate[i].name);
            }
        }

    }

    $scope.changegraph = function (type) {
        if (type == 1) {
            $scope.filtdata();
        } else {
            $scope.filtdataValue();
        }
    }

    $scope.filtdataValue = function () {
        
        finallistoflist = [];
        AlllistDPs = [];
        PenlistDps = [];
        delilistDPs = [];
        cncllistDps = [];
        RedisplistDps = [];
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var fd = { y: Allbydate[i].AllOrderAmount/1000, label: Allbydate[i].name };
                if (fd != null) {
                    AlllistDPs.push(fd);
                }
            }
        }
        if (AlllistDPs.length > 0) {
            var gd = $scope.chartdata(AlllistDPs, "#7f6084", "AllOrders");
            finallistoflist.push(gd);
        }
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var cd = { y: Allbydate[i].PendingOderAmount / 1000, label: Allbydate[i].name };
                if (cd != null) {
                    PenlistDps.push(cd);
                }
            }
        }
        if (PenlistDps.length > 0) {
            var gd = $scope.chartdata(PenlistDps, "#ffff99", "Pending");
            finallistoflist.push(gd);
        }
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var dd = { y: Allbydate[i].DeliveredOrderAmount / 1000, label: Allbydate[i].name };
                if (fd != null) {
                    delilistDPs.push(dd);
                }
            }
        }
        if (delilistDPs.length > 0) {
            var gd = $scope.chartdata(delilistDPs, "#86b402", "Delivered");
            finallistoflist.push(gd);
        }
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var cd = { y: Allbydate[i].CanceledOderAmount / 1000, label: Allbydate[i].name };
                if (cd != null) {
                    cncllistDps.push(cd);
                }
            }
        }
        if (cncllistDps.length > 0) {
            var gd = $scope.chartdata(cncllistDps, "#c24642", "Canceled");
            finallistoflist.push(gd);
        }
        for (var i = 0; i < Allbydate.length; i++) {
            if (Allbydate[i] != null) {
                var rd = { y: Allbydate[i].RedispatchedOrderAmount / 1000, label: Allbydate[i].name };
                if (rd != null) {
                    RedisplistDps.push(rd);
                }
            }
        }
        if (RedisplistDps.length > 0) {
            var gd = $scope.chartdata(RedisplistDps, "#369ead", "Redispatch");
            finallistoflist.push(gd);
        }

        if (finallistoflist.length > 0) {
            $scope.getChartOrder(finallistoflist, "Sales Values/1000");
        }
    }
 
    $scope.getChartOrder = function (graphdata, titl) {
        var chart = new CanvasJS.Chart("chartContainer",
       {
           title: {
               text: titl
           },
           animationEnabled: true,
           axisY: {
               title: titl
           },
           toolTip: {
               shared: true,
               contentFormatter: function (e) {
                   var content = " ";
                   for (var i = 0; i < e.entries.length; i++) {
                       var dboyname = e.entries[i].dataPoint.label;
                       if (Allbydate.length > 0) {
                           for (var l = 0; l < Allbydate.length; l++) {
                               if (Allbydate[l] !=null && Allbydate[l].name == dboyname) {
                                   content += Allbydate[l].name + "<br/>" + "Total Orders" + Allbydate[l].totalOrder + "<br/>";
                                   content += "AllOrderAmount:-" + Allbydate[l].AllOrderAmount + "<br/>";
                                   content += "<span style='color:#b3b300'>Pending:-" + Allbydate[l].Pending + "</span><br/>";
                                   content += "<span style='color:#b3b300'>PendingOrderAmount:-" + Allbydate[l].PendingOderAmount + "</span><br/>";
                                   content += "<span style='color:green'>Delivered:-" + Allbydate[l].Delivered + "</span><br/>";
                                   content += "<span style='color:green'>DeliveredOrderAmount:-" + Allbydate[l].DeliveredOrderAmount + "</span><br/>";
                                   content += "<span style='color:Red'>Canceled:-" + Allbydate[l].Canceled + "</span><br/>";
                                   content += "<span style='color:Red'>CanceledOderAmount:-" + Allbydate[l].CanceledOderAmount + "</span><br/>";
                                   content += "<span style='color:grey'>Redispatched:-" + Allbydate[l].Redispatched + "</span><br/>";
                                   content += "<span style='color:grey'>RedispatchedOrderAmount:-" + Allbydate[l].RedispatchedOrderAmount + "</span></br>";
                                   return content;
                               }
                           }
                       }
                   }

               }
           },
           legend: {
               verticalAlign: "bottom",
               horizontalAlign: "center"
           },
           theme: "theme3",
           data: graphdata
       });
        chart.render();
    }
}]);

