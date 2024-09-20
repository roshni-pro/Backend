'use strict';
app.controller('OrderDataController', ['$scope', 'OrderMasterService', 'OrderDetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', "DeliveryService", "WarehouseService", 'CityService',
    function ($scope, OrderMasterService, OrderDetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal, DeliveryService, WarehouseService, CityService) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
      {
            console.log("orderMasterController start loading OrderDetailsService");
                              
            $scope.cities = [];
            CityService.getcitys().then(function (results) {
                $scope.cities = results.data;
            }, function (error) {
            });

            $scope.warehouse = [];
            $scope.getWarehosues = function () {
                
                WarehouseService.getwarehouse().then(function (results) {
                    $scope.warehouse = results.data;
                    
                  }, function (error) {
                })
            };
            $scope.getWarehosues();
           
            /// Search function with assignment id by raj
            $scope.getSearchdata = function (startdate, enddate, WarehouseId) { // This would fetch the data on page change.  //In practice this should be in a factory.
               
                $scope.customers = [];
               
               
                var url = serviceBase + "api/OrderDispatchedMaster/OrderDataDownload" + "?datefrom=" + startdate + "&dateto=" + enddate +"&WarehouseId=" + $scope.WarehouseId;
                $http.get(url).success(function (response) {
                    
                    if (response.length == 0) {
                        alert("Not Found");
                    }
                    else {
                        
                        $scope.listMaster = response;  //ajax request to fetch data into vm.data
                        $scope.listMasterold = angular.copy(response.ordermaster);
                        console.log("get all Order:");
                        console.log($scope.customers);
                        $scope.orders = $scope.customers;
                        $scope.total_count = response.total_count;
                        $scope.tempuser = response.ordermaster;
                        $scope.exportData();
                    }
                });
            };

            // $scope.getSearchdata($scope.pageno, "all", "", "", "");                      
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A'
                });

                $('.input-group-addon').click(function () {
                    $('input[name="daterange"]').trigger("select");
                    //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

                });

            });

            $scope.Search = function (data) {

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

                $scope.getSearchdata($scope.dataforsearch.datefrom, $scope.dataforsearch.dateto, $scope.WarehouseId);

            };
           
            $scope.show = true;
            $scope.order = false;

            $scope.showalldetails = function () {
                $scope.order = !$scope.order;
                $scope.show = !$scope.show;
                // $scope.callmethoddetails();
            };

            
            $scope.exportData = function (data) {
                
                $scope.OrderByDate = $scope.listMaster;
                console.log("export");
                if ($scope.OrderByDate.length <= 0) {
                    alert("No data available between two date ")
                }
                else {

                    $scope.NewExportData = [];
                    for (var i = 0; i < $scope.OrderByDate.length; i++) {

                        var tts = {
                            CustomerId:'', SKCode:'', ShopName: '', WarehouseName: '', MobileNo: '', Address: '', NOOfOrder: '', TotalAmount: '',ExcutiveName: ''
                            
                        };
                        tts.CustomerId = $scope.OrderByDate[i].CustomerId;
                        tts.SKCode = $scope.OrderByDate[i].Skcode;
                        tts.ShopName = $scope.OrderByDate[i].ShopName;
                        tts.WarehouseName = $scope.OrderByDate[i].WarehouseName;
                        tts.MobileNo = $scope.OrderByDate[i].Customerphonenum;
                        tts.Address = $scope.OrderByDate[i].ShippingAddress;
                        tts.NOOfOrder = $scope.OrderByDate[i].OrderCount;
                        tts.TotalAmount = $scope.OrderByDate[i].TotalAmount;
                        tts.ExcutiveName = $scope.OrderByDate[i].SalesPerson;
                        
                        $scope.NewExportData.push(tts);
                    }
                    alasql.fn.myfmt = function (n) {
                        return Number(n).toFixed(2);
                    };
                }
                alasql('SELECT CustomerId,SKCode,ShopName,WarehouseName,Address,NOOfOrder,TotalAmount,ExcutiveName INTO XLSX("OrderData.xlsx",{headers:true}) FROM ?', [$scope.NewExportData]);

            };

//*************************************** New fuction for Advance Search*************************************************//

            $scope.getSearchADVdata = function (Demanddata) { // This would fetch the data on page change.  //In practice this should be in a factory.

                $scope.customers = [];

                var url = serviceBase + "api/OrderDispatchedMaster/OrderDataAdvance" + "?Year=" + Demanddata.Year + "&Cityid=" + Demanddata.Cityid + "&WarehouseId=" + Demanddata.WarehouseId;
                $http.get(url).success(function (response) {
                   
                    if (response.length == 0) {
                        alert("Not Found");
                    }
                    else {

                        $scope.newlistMaster = response;  //ajax request to fetch data into vm.data
                        $scope.newlistMasterold = angular.copy(response.ordermaster);
                        console.log("get all Order:");
                        console.log($scope.customers);
                        $scope.orders = $scope.customers;
                        $scope.total_count = response.total_count;
                        $scope.tempuser = response.ordermaster;
                        $scope.NewsearchExport();

                    }
                });
            };

            $scope.Searchdata = function (data) {

                $scope.datafornewsearch = { Year: "" };
                
                $scope.getSearchADVdata($scope.datafornewsearch.Year, $scope.WarehouseId);

            };

            //******************************************** Fuction for Advance NewsearchExport **********************************************************//

            $scope.NewsearchExport = function (data) {
              
                $scope.newOrderByDate = $scope.newlistMaster;
            console.log("export");
            if ($scope.newOrderByDate.length <= 0) {
                alert("No data available On this year ");
            }
            else {

                $scope.NewSearchExportData = [];
                for (var i = 0; i < $scope.newOrderByDate.length; i++) {

                    var tts = {
                        skcode: '', shopname: '', warehouseid: '', warehousename: '', executiveid: '', executivename: '', Jan: '', Feb: '', Mar: '', Apr: '', May: '', June: '', July: '', Aug: '', Sep: '', Oct: '', Nov: '', Dec: '', totalorderCount: '', TotalAmount: ''

                    };
                    tts.skcode = $scope.newOrderByDate[i].skcode;
                    tts.shopname = $scope.newOrderByDate[i].shopname;
                    tts.warehouseid = $scope.newOrderByDate[i].warehouseid;
                    tts.warehousename = $scope.newOrderByDate[i].warehousename;
                    tts.executiveid = $scope.newOrderByDate[i].executiveid;
                    tts.executivename = $scope.newOrderByDate[i].executivename;                                        
                    tts.Jan = $scope.newOrderByDate[i].Jan;
                    tts.Feb = $scope.newOrderByDate[i].Feb;
                    tts.Mar = $scope.newOrderByDate[i].Mar;
                    tts.Apr = $scope.newOrderByDate[i].Apr;
                    tts.May = $scope.newOrderByDate[i].May;
                    tts.June = $scope.newOrderByDate[i].June;
                    tts.July = $scope.newOrderByDate[i].July;
                    tts.Aug = $scope.newOrderByDate[i].Aug;
                    tts.Sep = $scope.newOrderByDate[i].Sep;
                    tts.Oct = $scope.newOrderByDate[i].Oct;
                    tts.Nov = $scope.newOrderByDate[i].Nov;
                    tts.Dec = $scope.newOrderByDate[i].Dec;
                    tts.totalorderCount = $scope.newOrderByDate[i].totalorderCount;
                    tts.TotalAmount = $scope.newOrderByDate[i].TotalAmount;
                  
                    $scope.NewSearchExportData.push(tts);
                }
                alasql.fn.myfmt = function (n) {
                    return Number(n).toFixed(2);
                };
            }
                alasql('SELECT skcode,shopname,warehouseid,warehousename,executiveid,executivename,Jan,Feb,Mar,Apr,May,June,July,Aug,Sep,Oct,Nov,Dec,totalorderCount,TotalAmount INTO XLSX("Year.xlsx",{headers:true}) FROM ?', [$scope.NewSearchExportData]);
            };   
        }
    }]);

