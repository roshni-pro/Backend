'use strict'
app.controller('HubPhaseController', ['$scope', "$http", "$modal", "ngTableParams", "WarehouseService", "CityService", function ($scope, $http, $modal, ngTableParams, WarehouseService, CityService) {

    // get all Cities
    //$scope.citys = [];
    //CityService.getcitys().then(function (results) {
    //    $scope.citys = results.data;
    //}, function (error) { });

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
    //$scope.GetWarehouses = function (warehouse) {
    //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.warehouses = response;
    //        });
    //}; --warehouse standardization

    $scope.GetWarehouses = function (warehouse) {
        var url = serviceBase + 'api/DeliveyMapping/GetWarehouseCommonByRegion?RegionId=' + warehouse;
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


    //get hub  for selected city
    $scope.getWarehouse = function (id) {
        WarehouseService.warehousecitybased(id).then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) { });
    };

    $scope.Open = function (item) {

        //
        console.log(" Dialog called ");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "HubPhaseModel.html",
                controller: "HubPhaseInstanceCtrl", resolve: { state: function () { return item } }
            }), modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");

                });

    };


    // $scope.PhaseData = {};
    // Get Calculated Data
    $scope.getPhaseData = function (data) {
      
        var datatopost =
            {
                Cityid: data.Cityid,
                WarehouseId: data.WarehouseId,
                MonthYear: data.MonthYear
            };
        $scope.storeData = [];

        var url = serviceBase + "api/HubPhase";
        $http.post(url, datatopost).success(function (results) {
            // 
            //Scale
            $scope.storeData = results;
            $scope.NumberofSignup = results.NumberofSignup;
            $scope.ActiveStores = results.ActiveStores;
            $scope.Monthsale = results.Monthsale;
            $scope.TotalOrderTill = results.TotalOrderTill;
            //Product reliance
            $scope.Monthorder = results.Monthorder;
            $scope.freqOrder = results.freqOrder;
            $scope.AverageOrdervalue = results.AverageOrdervalue;
            $scope.AvgLineItem = results.AverageLineItem;
            $scope.onlineOrder = results.onlineOrder;
            //Kisan Kirana
            $scope.KisanKiranaActive = results.KisanKiranaActive;
            $scope.KisanKiranaSale = results.KisanKiranaSale;
            $scope.KisanOrderFreq = results.KisanOrderFreq;
            //Service KPIs:
            $scope.PostOrderCancel = results.PerOfPostOrdCancel;
            //Sourcing :
            $scope.ActiveArticles = results.ActiveArticles;
            $scope.SoldArtciles = results.SoldArtciles;
            $scope.Numberofvendors = results.Numberofvendors;
            $scope.ActiveVendors = results.ActiveVendors;
            //Cost:
            $scope.Inventorydays = results.Inventorydays;


            //Number of sign Up
            if ($scope.NumberofSignup < 2000) {
                //
                $scope.NumberofSignup = results.NumberofSignup;
                $scope.NumberofSignup1 = '-';
                $scope.NumberofSignup2 = '-';
                $scope.NumberofSignup3 = '-';

            } else if ($scope.NumberofSignup >= 2000 && $scope.NumberofSignup < 3000) {
                $scope.NumberofSignup = '-';
                $scope.NumberofSignup1 = results.NumberofSignup;
            }
            else if ($scope.NumberofSignup >= 3000 && $scope.NumberofSignup < 4000) {
                $scope.NumberofSignup = '-';
                $scope.NumberofSignup1 = '-';
                $scope.NumberofSignup2 = results.NumberofSignup;

            }
            else {
                $scope.NumberofSignup = '-';
                $scope.NumberofSignup1 = '-';
                $scope.NumberofSignup2 = '-';
                $scope.NumberofSignup3 = results.NumberofSignup;
            }

            //Active Stores
            if ($scope.ActiveStores < 600) {
                //
                $scope.ActiveStores = results.ActiveStores;
                $scope.ActiveStores1 = '-';

            } else if ($scope.ActiveStores >= 600 && $scope.ActiveStores < 1000) {
                $scope.ActiveStores = '-';
                $scope.ActiveStores1 = results.ActiveStores;


            }
            else if ($scope.ActiveStores >= 1000 && $scope.ActiveStores < 1500) {
                $scope.ActiveStores = '-';
                $scope.ActiveStores1 = '-';
                $scope.ActiveStores2 = results.ActiveStores;


            }
            else {
                $scope.ActiveStores = '-';
                $scope.ActiveStores1 = '-';
                $scope.ActiveStores2 = '-';
                $scope.ActiveStores3 = results.ActiveStores;
            }

            //Sales
            if ($scope.Monthsale < 200) {
                // 
                $scope.Monthsale = results.Monthsale;
                $scope.Monthsale1 = '-';
                $scope.Monthsale2 = '-';
                $scope.Monthsale3 = '-';

            } else if ($scope.Monthsale >= 200 && $scope.Monthsale < 300) {
                $scope.Monthsale = '-';
                $scope.Monthsale1 = results.Monthsale;


            }
            else if ($scope.Monthsale >= 300 && $scope.Monthsale < 400) {
                $scope.Monthsale = '-';
                $scope.Monthsale1 = '-';
                $scope.Monthsale2 = results.Monthsale;

            } else {
                $scope.Monthsale = '-';
                $scope.Monthsale1 = '-';
                $scope.Monthsale2 = '-';
                $scope.Monthsale3 = results.Monthsale;
            }

            //Total order
            //if ($scope.TotalOrderTill < 2500) {
            //    //  
            //    $scope.TotalOrderTill = results.TotalOrderTill;
            //    $scope.TotalOrderTill1 = '-';
            //    $scope.TotalOrderTill2 = '-';
            //    $scope.TotalOrderTill3 = '-';

            //} else if ($scope.TotalOrderTill >= 2500 && $scope.TotalOrderTill < 4000) {
            //    $scope.TotalOrderTill = '-';
            //    $scope.TotalOrderTill1 = results.TotalOrderTill;


            //}
            //else if ($scope.TotalOrderTill >= 4000 && $scope.TotalOrderTill < 6000) {
            //    $scope.TotalOrderTill = '-';
            //    $scope.TotalOrderTill1 = '-';
            //    $scope.TotalOrderTill2 = results.TotalOrderTill;

            //} else {
            //    $scope.TotalOrderTill = '-';
            //    $scope.TotalOrderTill1 = '-';
            //    $scope.TotalOrderTill2 = '-';
            //    $scope.TotalOrderTill3 = results.TotalOrderTill;
            //}

            //Monthly orders
            if ($scope.Monthorder < 2500) {

                $scope.Monthorder = results.Monthorder;
                $scope.Monthorder1 = '-';
                $scope.Monthorder2 = '-';
                $scope.Monthorder3 = '-';

            } else if ($scope.Monthorder >= 2500 && $scope.Monthorder < 4500) {
                $scope.Monthorder = '-';
                $scope.Monthorder1 = results.Monthorder;


            }
            else if ($scope.Monthorder >= 4500 && $scope.Monthorder < 8000) {
                $scope.Monthorder = '-';
                $scope.Monthorder1 = '-';
                $scope.Monthorder2 = results.Monthorder;

            } else {

                $scope.Monthorder = '-';
                $scope.Monthorder1 = '-';
                $scope.Monthorder2 = '-';
                $scope.Monthorder3 = results.Monthorder;

            }

            //Ferq of Order
            if ($scope.freqOrder < 4.20) {
                // 
                $scope.freqOrder = results.freqOrder;
                $scope.freqOrder1 = '-';
                $scope.freqOrder2 = '-';
                $scope.freqOrder3 = '-';

            } else if ($scope.freqOrder >= 4.20 && $scope.freqOrder < 4.50) {
                $scope.freqOrder = '-';
                $scope.freqOrder1 = results.freqOrder;
            }
            else if ($scope.freqOrder >= 4.50 && $scope.freqOrder < 5.30) {
                $scope.freqOrder = '-';
                $scope.freqOrder1 = '-';
                $scope.freqOrder2 = results.freqOrder;

            } else {
                $scope.freqOrder = '-';
                $scope.freqOrder1 = '-';
                $scope.freqOrder2 = '-';
                $scope.freqOrder3 = results.freqOrder;

            }
            //Avg order value
            if ($scope.AverageOrdervalue < 5500.0) {
                $scope.AverageOrdervalue = results.AverageOrdervalue;
                $scope.AverageOrdervalue1 = '-';
                $scope.AverageOrdervalue2 = '-';
                $scope.AverageOrdervalue3 = '-';

            } else if ($scope.AverageOrdervalue >= 5500.0 && $scope.AverageOrdervalue < 7000.00) {
                $scope.AverageOrdervalue = '-';
                $scope.AverageOrdervalue1 = results.AverageOrdervalue;


            }
            else if ($scope.AverageOrdervalue >= 7000.00 && $scope.AverageOrdervalue < 10000.00) {
                $scope.AverageOrdervalue = '-';
                $scope.AverageOrdervalue1 = '-';
                $scope.AverageOrdervalue2 = results.AverageOrdervalue;

            } else {

                $scope.AverageOrdervalue = '-';
                $scope.AverageOrdervalue1 = '-';
                $scope.AverageOrdervalue2 = '-';
                $scope.AverageOrdervalue3 = results.AverageOrdervalue;

            }

            //Avg Line item
            if ($scope.AvgLineItem < 6) {
                $scope.AvgLineItem = results.AverageLineItem;
                $scope.AvgLineItem1 = '-';
                $scope.AvgLineItem2 = '-';
                $scope.AvgLineItem3 = '-';

            } else if ($scope.AvgLineItem >= 6 && $scope.AvgLineItem < 8) {
                $scope.AvgLineItem = '-';
                $scope.AvgLineItem1 = results.AverageLineItem;


            }
            else if ($scope.AvgLineItem >= 8 && $scope.AvgLineItem < 10) {
                $scope.AvgLineItem = '-';
                $scope.AvgLineItem1 = '-';
                $scope.AvgLineItem2 = results.AverageLineItem;

            } else {

                $scope.AvgLineItem = '-';
                $scope.AvgLineItem1 = '-';
                $scope.AvgLineItem2 = '-';
                $scope.AvgLineItem3 = results.AverageLineItem;

            }

            //Online Order percentage
            if ($scope.onlineOrder < 20) {
                $scope.onlineOrder = results.onlineOrder;
                $scope.onlineOrder1 = '-';
                $scope.onlineOrder2 = '-';
                $scope.onlineOrder3 = '-';

            } else if ($scope.onlineOrder >= 20 && $scope.onlineOrder < 30) {
                $scope.onlineOrder = '-';
                $scope.onlineOrder1 = results.onlineOrder;


            }
            else if ($scope.onlineOrder >= 30 && $scope.onlineOrder < 40) {
                $scope.onlineOrder = '-';
                $scope.onlineOrder1 = '-';
                $scope.onlineOrder2 = results.onlineOrder;

            } else {

                $scope.onlineOrder = '-';
                $scope.onlineOrder1 = '-';
                $scope.onlineOrder2 = '-';
                $scope.onlineOrder3 = results.onlineOrder;

            }

            //Kisan Kirana Activation
            if ($scope.KisanKiranaActive < 360) {
                $scope.KisanKiranaActive = results.KisanKiranaActive;
                $scope.KisanKiranaActive1 = '-';
                $scope.KisanKiranaActive2 = '-';
                $scope.KisanKiranaActive3 = '-';

            } else if ($scope.KisanKiranaActive >= 360 && $scope.KisanKiranaActive < 600) {
                $scope.KisanKiranaActive = '-';
                $scope.KisanKiranaActive1 = results.KisanKiranaActive;


            }
            else if ($scope.KisanKiranaActive >= 600 && $scope.KisanKiranaActive < 900) {
                $scope.KisanKiranaActive = '-';
                $scope.KisanKiranaActive1 = '-';
                $scope.KisanKiranaActive2 = results.KisanKiranaActive;

            } else {

                $scope.KisanKiranaActive = '-';
                $scope.KisanKiranaActive1 = '-';
                $scope.KisanKiranaActive2 = '-';
                $scope.KisanKiranaActive3 = results.KisanKiranaActive;

            }

            //Kisan kirana Sale
            if ($scope.KisanKiranaSale < 25) {
                $scope.KisanKiranaSale = results.KisanKiranaSale;
                $scope.KisanKiranaSale1 = '-';
                $scope.KisanKiranaSale2 = '-';
                $scope.KisanKiranaSale3 = '-';

            } else if ($scope.KisanKiranaSale >= 25 && $scope.KisanKiranaSale < 75) {
                $scope.KisanKiranaSale = '-';
                $scope.KisanKiranaSale1 = results.KisanKiranaSale;


            }
            else if ($scope.KisanKiranaSale >= 75 && $scope.KisanKiranaSale < 125) {
                $scope.KisanKiranaSale = '-';
                $scope.KisanKiranaSale1 = '-';
                $scope.KisanKiranaSale2 = results.KisanKiranaSale;

            } else {

                $scope.KisanKiranaSale = '-';
                $scope.KisanKiranaSale1 = '-';
                $scope.KisanKiranaSale2 = '-';
                $scope.KisanKiranaSale3 = results.KisanKiranaSale;

            }
            //Kisan Kirana Order Ferq.
            if ($scope.KisanOrderFreq < 1.50) {
                $scope.KisanOrderFreq = results.KisanOrderFreq;
                $scope.KisanOrderFreq1 = '-';
                $scope.KisanOrderFreq2 = '-';
                $scope.KisanOrderFreq3 = '-';

            } else if ($scope.KisanOrderFreq >= 1.5 && $scope.KisanOrderFreq < 2.5) {
                $scope.KisanOrderFreq = '-';
                $scope.KisanOrderFreq1 = results.KisanOrderFreq;


            }
            else if ($scope.KisanOrderFreq >= 2.5 && $scope.KisanOrderFreq < 3.0) {
                $scope.KisanOrderFreq = '-';
                $scope.KisanOrderFreq1 = '-';
                $scope.KisanOrderFreq2 = results.KisanOrderFreq;

            } else {

                $scope.KisanOrderFreq = '-';
                $scope.KisanOrderFreq1 = '-';
                $scope.KisanOrderFreq2 = '-';
                $scope.KisanOrderFreq3 = results.KisanOrderFreq;

            }

            //Post Order Cancellation %
            if ($scope.PostOrderCancel < 10) {
                $scope.PostOrderCancel = '-';
                $scope.PostOrderCancel1 = '-';
                $scope.PostOrderCancel2 = '-';
                $scope.PostOrderCancel3 = results.PerOfPostOrdCancel;

            } else if ($scope.PostOrderCancel > 10 && $scope.PostOrderCancel < 15) {
                $scope.PostOrderCancel = '-';
                $scope.PostOrderCancel1 = '-';
                $scope.PostOrderCancel2 = results.PerOfPostOrdCancel;
            }
            else if ($scope.PostOrderCancel > 15 && $scope.PostOrderCancel < 20) {
                $scope.PostOrderCancel = '-';
                $scope.PostOrderCancel1 = results.PerOfPostOrdCancel;

            } else {
                $scope.PostOrderCancel = results.PerOfPostOrdCancel;
                $scope.PostOrderCancel1 = '-';
                $scope.PostOrderCancel2 = '-';
                $scope.PostOrderCancel3 = '-';
            }

            // Active Articles
            if ($scope.ActiveArticles < 700) {
                $scope.ActiveArticles = results.ActiveArticles;
                $scope.ActiveArticles1 = '-';
                $scope.ActiveArticles2 = '-';
                $scope.ActiveArticles3 = '-';

            } else if ($scope.ActiveArticles > 700 && $scope.ActiveArticles < 1200) {
                $scope.ActiveArticles = '-';
                $scope.ActiveArticles1 = results.ActiveArticles;


            }
            else if ($scope.ActiveArticles > 1200 && $scope.ArticleData < 1500) {
                $scope.ActiveArticles = '-';
                $scope.ActiveArticles1 = '-';
                $scope.ActiveArticles2 = results.ActiveArticles;

            } else {

                $scope.ActiveArticles = '-';
                $scope.ActiveArticles1 = '-';
                $scope.ActiveArticles2 = '-';
                $scope.ActiveArticles3 = results.ActiveArticles;

            }

            //Sold Articles
            if ($scope.SoldArtciles < 350) {
                $scope.SoldArtciles = results.SoldArtciles;
                $scope.SoldArtciles1 = '-';
                $scope.SoldArtciles2 = '-';
                $scope.SoldArtciles3 = '-';

            } else if ($scope.SoldArtciles > 350 && $scope.SoldArtciles < 500) {
                $scope.SoldArtciles = '-';
                $scope.SoldArtciles1 = results.SoldArtciles;


            }
            else if ($scope.SoldArtciles > 500 && $scope.SoldArtciles < 750) {
                $scope.SoldArtciles = '-';
                $scope.SoldArtciles1 = '-';
                $scope.SoldArtciles2 = results.SoldArtciles;

            } else {

                $scope.SoldArtciles = '-';
                $scope.SoldArtciles1 = '-';
                $scope.SoldArtciles2 = '-';
                $scope.SoldArtciles3 = results.SoldArtciles;

            }

            //No of Vendors

            if ($scope.Numberofvendors <= 20) {
                $scope.Numberofvendors = results.Numberofvendors;
                $scope.Numberofvendors1 = '-';
                $scope.Numberofvendors2 = '-';
                $scope.Numberofvendors3 = '-';

            } else if ($scope.Numberofvendors > 20 && $scope.Numberofvendors <= 30) {
                $scope.Numberofvendors = '-';
                $scope.Numberofvendors1 = results.Numberofvendors;


            }
            else if ($scope.Numberofvendors > 30 && $scope.Numberofvendors <= 50) {
                $scope.Numberofvendors = '-';
                $scope.Numberofvendors1 = '-';
                $scope.Numberofvendors2 = results.Numberofvendors;

            } else {

                $scope.Numberofvendors = '-';
                $scope.Numberofvendors1 = '-';
                $scope.Numberofvendors2 = '-';
                $scope.Numberofvendors3 = results.Numberofvendors;

            }

            //Active Vendors
            if ($scope.ActiveVendors <= 21) {
                $scope.ActiveVendors = results.ActiveVendors;
                $scope.ActiveVendors1 = '-';
                $scope.ActiveVendors2 = '-';
                $scope.ActiveVendors3 = '-';

            } else if ($scope.ActiveVendors > 21 && $scope.ActiveVendors < 35) {
                $scope.ActiveVendors = '-';
                $scope.ActiveVendors1 = results.ActiveVendors;


            }
            else if ($scope.ActiveVendors >= 35 && $scope.ActiveVendors < 56) {
                $scope.ActiveVendors = '-';
                $scope.ActiveVendors1 = '-';
                $scope.ActiveVendors2 = results.ActiveVendors;

            } else {

                $scope.ActiveVendors = '-';
                $scope.ActiveVendors1 = '-';
                $scope.ActiveVendors2 = '-';
                $scope.ActiveVendors3 = results.ActiveVendors;

            }

            //Inventory Days:
            if ($scope.Inventorydays < 8) {
                $scope.Inventorydays = '-';
                $scope.Inventorydays1 = '-';
                $scope.Inventorydays2 = '-';
                $scope.Inventorydays3 = results.Inventorydays;

            } else if ($scope.Inventorydays >= 8 && $scope.Inventorydays <= 10) {
                $scope.Inventorydays = '-';
                $scope.Inventorydays1 = '-';
                $scope.Inventorydays2 = results.Inventorydays;
            }
            else if ($scope.Inventorydays > 10 && $scope.Inventorydays <= 14) {
                $scope.Inventorydays = '-';
                $scope.Inventorydays1 = results.Inventorydays;
            } else {

                $scope.Inventorydays = results.Inventorydays;
                $scope.Inventorydays1 = '-';
                $scope.Inventorydays2 = '-';
                $scope.Inventorydays3 = '-';

            }



        });

    };

    $scope.exportData = function () {
       // 
        console.log($scope.storeData);
        alasql('SELECT NumberofSignup,ActiveStores,Monthsale,TotalOrderTill,Monthorder,freqOrder,AverageOrdervalue,AvgLineItem,onlineOrder,KisanKiranaActive,KisanKiranaSale,KisanOrderFreq INTO XLSX("HubPhase.xlsx",{headers:true}) FROM ?', [$scope.storeData]);
    };


}]);
app.controller("HubPhaseInstanceCtrl", ["$scope", '$http', 'ngAuthSettings', "CategoryService", "$modalInstance", "state", function ($scope, $http, ngAuthSettings, CategoryService, $modalInstance) {
    console.log("HubPhaseInstanceCtrl");

    $scope.Categories = [];
    CategoryService.getcategorys().then(function (results) {

        $scope.Categories = results.data;
    }, function (error) { });

    $scope.Categorydata = [];
    $scope.getpercentagedata = function (Categoryid) {
        var url = serviceBase + "api/HubPhase/Getpercentage?Categoryid=" + Categoryid;
        $http.get(url).success(function (data) {
            $scope.Categorydata = data;
        });
    };
    $scope.result = function (data) {
        $scope.total = (data.share / data.TAM) * 100;
    };


    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
        },

        $scope.add = function (data) {
            // 
            $scope.total = (data.share / data.TAM) * 100;
            $scope.Categorydata = [];
            var url = serviceBase + "api/HubPhase/AddCategoryMarketShare";
            var dataToPost =
                {
                    Categoryid: data.Categoryid,
                    WarehouseId: data.WarehouseId,
                    TAM: data.TAM,
                    MS: data.share,
                    Percentage: $scope.total
                };
            $http.post(url, dataToPost)
                .success(function (data) {
                });
        };


}])