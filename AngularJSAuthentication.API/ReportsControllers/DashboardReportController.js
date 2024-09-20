'use strict'
app.controller('DashboardReportController', ['$scope', "$http", "ngTableParams", "WarehouseService", function ($scope, $http, ngTableParams, WarehouseService) {
    
    ////for reload page once logied in 
    function reload() {
        localStorage.hasReloaded = true;
        window.location.reload();
    }
    if (!localStorage.hasReloaded || localStorage.hasReloaded == "false") {

        reload();
    }

    //get warehosueList
    $scope.warehouse = [];
    WarehouseService.getwarehouse().then(function (results) {

        $scope.warehouse = results.data;
    }, function (error) {
    });


   // $scope.WarehouseId = 1;

    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    //$scope.totalsell = {};
    //function convertDate(inputFormat) {
    //    function pad(s) { return (s < 10) ? '0' + s : s; }
    //    var d = new Date(inputFormat);
    //    return [d.getFullYear(), pad(d.getMonth() + 1), pad(d.getDate())].join('-');
    //}    


    //$scope.getWarehosues = function (warehouse) {
    //    $scope.warehouse = [];
    //    $scope.examplemodel = [];
    //    $(".overlayss").css({ "visibility": "hidden" });
    //    var url = serviceBase + 'api/Warehouse/getSpecificWarehousesid?regionId=' + warehouse;
    //    $http.get(url)
    //        .success(function (response) {
    //            if (response.length > 0) {
    //                for (var a = 0; a < response.length; a++) {
    //                    response[a].WarehouseName = response[a].WarehouseName + " " + response[a].CityName;
    //                }
    //                $scope.warehouse = response;
    //            }
    //            $scope.TActiveCount($scope.warehouse);
    //            var url = serviceBase + "api/DashboardReport";
    //            $scope.FilterData = [];
    //            $http.post(url, $scope.warehouse)
    //                .success(function (data) {
    //                    $scope.FilterData = data;
    //                })
    //                .error(function (data) {
    //                });
    //        });
    //}

    //$scope.Allregions = [];
    //$scope.GetAllRegions = function (zone) {
    //    $(".overlayss").css({ "visibility": "hidden" });
    //    var url = serviceBase + 'api/inventory/GetRegionAll';
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.Allregions = response;
    //        });
    //};
    //$scope.GetAllRegions();


    //$scope.GetWarehouses = function (regionId) {
    //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + regionId;
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.warehouses = response;
    //           //$scope.WarehouseId = $scope.warehouses[0].WarehouseId;

    //        });
    //}



    //$scope.warehouses = [];
    //$scope.GetWarehouses = function (warehouse) {
    //    $(".overlayss").css({ "visibility": "hidden" });
    //    var url = serviceBase + 'api/Warehouse/getSpecificWarehousesid?regionId=' + warehouse;
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.warehouses = response;
    //           //$scope.WarehouseId = $scope.warehouses[0].WarehouseId;
 
    //        });
    //};
    //get warehosueList

    $scope.warehouse = [];
    WarehouseService.getwarehouse().then(function (results) {
        $scope.warehouse = [];
        $scope.warehouse = results.data;
    }, function (error) {
    });

    //$scope.examplemodel = [];
    //$scope.exampledata = $scope.warehouse;
    //$scope.examplesettings = {
    //    displayProp: 'WarehouseName', idProp: 'WarehouseId',
    //    scrollableHeight: '300px',
    //    scrollableWidth: '450px',
    //    enableSearch: true,
    //    scrollable: true
    //};

    //$scope.getsell = function () {
    //    $(".overlaysstotalsell").css('visibility', 'visible');
    //    var url = serviceBase + "api/DashboardReport/totalSell";
    //    $http.get(url).success(function (results) {
           
    //        var str = results.replace(/\"/g, "");
    //        $scope.totalTodaysell = str.substr(0, str.indexOf(' ')); // "72"
    //        $scope.totalMonthsell= str.substr(str.indexOf(' ') + 1); // "tocirah sneab"
    //        $(".overlaysstotalsell").css('visibility', 'hidden');
            
    //    });
    //};
    //$scope.getsell();
    $scope.getsellDet = function ()
    {
        $scope.MonthSales = [];
        $(".overlaysstotalsell").css('visibility', 'visible');
        var url = serviceBase + "api/DashboardReport/MonthSale";
        $http.get(url).success(function (results) {

            $scope.MonthSales = results;
            //var str = results.replace(/\"/g, "");
            //$scope.totalTodaysellDet = str.substr(0, str.indexOf(' ')); // "72"
            //$scope.totalMonthsellDet = str.substr(str.indexOf(' ') + 1); // "tocirah sneab"
            $(".overlaysstotalsell").css('visibility', 'hidden');

        });
    };
    $scope.getsellDet();

    //$scope.selectedwarehouse = function () {
    //    var WarehouseId = [];
    //    _.each($scope.examplemodel, function (o2) {

    //        console.log(o2);
    //        for (var i = 0; i < $scope.warehouse.length; i++) {
    //            if ($scope.warehouse[i].WarehouseId == o2.id) {
    //                var Row =
    //                {
    //                    "WarehouseId": o2.id
    //                };
    //                WarehouseId.push(Row);
    //            }
    //        }
    //    });
    //    if (WarehouseId == 0) {
    //        alert("Please Select Warehouse");
    //        return;
    //    }
    //    var url = serviceBase + "api/DashboardReport";
       
    //    $scope.FilterData = [];
    //   // console.log(dataToPost);
    //    $http.post(url, WarehouseId)
    //        .success(function (data) {
    //            $scope.FilterData = data;
    //        })
    //        .error(function (data) {
    //        });
    //    $scope.getsell();
    //    $scope.getsellDet();
    //    $scope.custdata = null;
    //    $scope.Allbydate = [];
    //    $scope.TActiveCount(WarehouseId);
    //    $scope.WarehouseId = 0;
    //};

    ////CaseNotificationCode
    //$scope.SearchNotification = function () {
       
    //    var url = serviceBase + 'api/Cases/Notification?Assignto=' + $scope.UserRole.role;
    //    $http.get(url).success(function (results) {
    //        $scope.AllCases = results;
    //    });
    //}
    ////end

    //$scope.SearchNotification();
    //$scope.getCustData = function () {
    //    var url = serviceBase + "api/DashboardReport?Warehouseid=" + $scope.WarehouseId;
    //    $http.get(url).success(function (results) {
           
    //        if (results != null)
    //        {
    //            $scope.custdata = results;
                
    //        }
 
    //    });
    //};
    //$scope.signup = [];
    //$scope.activer = [];
    //$scope.TActiveCount = function (id) {
    //    var warehouses = id;
    //    if (warehouses != undefined) {
    //        var url = serviceBase + "api/DashboardReport/TotalActiveCustomer";
    //        $http.post(url, warehouses)
    //            .success(function (results) {
    //                if (results != null) {

    //                    $scope.signup = results[0].TotalSignup;
    //                    $scope.activer = results[0].ActiveRetailer;

    //                }
    //            })
    //            .error(function (data) {
    //            });
    //    }
    //    else
    //    {
    //        var url = serviceBase + "api/DashboardReport/TotalActiveCustomer?warehouseid=0";
    //        $http.get(url).success(function (results) {

    //            if (results != null) {
                    
    //                $scope.signup = results[0].TotalSignup;
    //                $scope.activer = results[0].ActiveRetailer;

    //            }

    //        });
    //    }
    //};
 
     
  
    //$scope.PostDashData = function () {
    //    var WarehouseId = [];
    //    _.each($scope.examplemodel, function (o2) {

    //        console.log(o2);
    //        for (var i = 0; i < $scope.warehouse.length; i++) {
    //            if ($scope.warehouse[i].WarehouseId == o2.id) {
    //                var Row =
    //                {
    //                    "WarehouseId": o2.id
    //                };
    //                WarehouseId.push(Row);
    //            }
    //        }
    //    });
    //    if (WarehouseId.length == 0) {
    //        alert("Please Select Warehouse");
    //        return;
    //    }
    //    var url = serviceBase + "api/DashboardReport/updateInventory";
    //    $http.post(url, WarehouseId).success(function (response) {
    //        if (response)
    //        { alert('Inventory turnover Calculated ') }
    //        else
    //        { alert('Not Done'); }         
    //    });

       
    //};
    $scope.Fillrate = 0;
    $scope.Monthfillrate = function (WarehouseId) {
        $scope.Fillrate = 0;

        $(".overlayss").css('visibility', 'visible');
        var url = serviceBase + "api/DashboardReport/fillrate?WarehouseId=" + WarehouseId;
        $http.post(url).success(function (result) {

            if (result) {
                $(".overlayss").css({ "visibility": "hidden" });
                $scope.Fillrate = result;
            }
            else
            {
                alert("No data available Month Monthfillrate");
                $(".overlayss").css({ "visibility": "hidden" });
                $scope.Fillrate = 0;
            }
        });
    };
    $scope.Monthfillrate(0);
    ////for date conversion
    //function convertDate(inputFormat) {
    //    var month = new Array();
    //    month[0] = "January";
    //    month[1] = "February";
    //    month[2] = "March";
    //    month[3] = "April";
    //    month[4] = "May";
    //    month[5] = "June";
    //    month[6] = "July";
    //    month[7] = "August";
    //    month[8] = "September";
    //    month[9] = "October";
    //    month[10] = "November";
    //    month[11] = "December";
    //    function pad(s) { return (s < 10) ? '0' + s : s; }
    //    var d = new Date(inputFormat);
    //    var n = month[d.getMonth()];
    //    return n;
    //}
}]);