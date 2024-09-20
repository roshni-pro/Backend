'use strict';
app.controller('EpayLetterController', ['$scope', "$filter", "$http", "ngTableParams", 'FileUploader', '$modal', '$log', 'WarehouseService', 'CityService', function ($scope, $filter, $http, ngTableParams, FileUploader, $modal, $log, WarehouseService, CityService) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'))

    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A',
        });
    });




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
    $scope.currentPageStores = {};;
    $scope.cities = [];
    CityService.getcitys().then(function (results) {
        $scope.cities = results.data;
    }, function (error) {
    });
    $scope.warehouse = [];
    $scope.getWarehosues = function (cityid) {
        $http.get("/api/Warehouse/GetWarehouseCity/?CityId=" + cityid).then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) {
        })
    };
    $scope.actions = [];
    $scope.getData = function () {

        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = f.val();
        var end = g.val();

        $http.get("/api/EpayLetter/GetAllDataToExport/?start=" + start + "&end=" + end).then(function (results) {

            $scope.actions = results.data;
            $scope.callmethod();
            alasql('SELECT Skcode,ShopName,FirmType,ProprietorFirstName,ProprietorLastName,Gender,Mobile,WhatsAppNumber,Email,DOB,PAN_No,Country,State,City,WarehouseName,PostalCode,CreatedDate INTO XLSX("Epayletterdetail.xlsx",{headers:true}) FROM ?', [$scope.actions]);
            console.log($scope.actions);
        }, function (error) {
        });
    }
    $scope.Getrbldata = [];
    $scope.Getrbldata = function () {
        $http.get(serviceBase + 'api/Myudhar/getall').then(function (results) {
            $scope.Getrbldata = results.data; //ajax request to fetch data into vm.data 
        });
    };
    $scope.Getrbldata();
    $scope.firsttimeorder = [];
    $scope.ExportAllDataOrder = function () {
        $scope.CustInfo = $scope.Getrbldata;
        alasql('SELECT Name,WarehouseName,cityName,SkCode,Mobile,PanCardNo,Address,DOB,postalcode,AnnualTurnOver,BusinessVintage,CreatedDate INTO XLSX("CustomerDetail.xlsx",{headers:true}) FROM ?', [$scope.CustInfo]);

    };

    $scope.openimg = function (data) {
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "imageView.html",
                controller: "ImageControllerPan", resolve: { object: function () { return data } }
            }), modalInstance.result.then(function (data) {
            },
                function () { })
    };
    $scope.openAddproff = function (data) {
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "Addproff.html",
                controller: "ImageControllerAdd", resolve: { object: function () { return data } }
            }), modalInstance.result.then(function (data) {
            },
                function () { })
    };
    $scope.openBackImg = function (data) {
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "Backimage.html",
                controller: "ImageControllerBack", resolve: { object: function () { return data } }
            }), modalInstance.result.then(function (data) {
            },
                function () { })
    };
    $scope.callmethod = function () {

        var init;
        return $scope.stores = $scope.actions,

            $scope.searchKeywords = "",
            $scope.filteredStores = [],
            $scope.row = "",

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
            },

            $scope.onFilterChange = function () {

                console.log("onFilterChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
            },

            $scope.onNumPerPageChange = function () {
                console.log("onNumPerPageChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.search = function () {
                console.log("search");
                console.log($scope.stores);
                console.log($scope.searchKeywords);

                return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
            },

            $scope.order = function (rowName) {
                console.log("order"); console.log($scope.stores);
                return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
            },

            $scope.numPerPageOpt = [3, 5, 10, 20],
            $scope.numPerPage = $scope.numPerPageOpt[2],
            $scope.currentPage = 1,
            $scope.currentPageStores = [],
            (init = function () {
                return $scope.search(), $scope.select($scope.currentPage)
            })

                ()


    }
}]);
app.controller("ImageControllerPan", ["$scope", "$modalInstance", "object", '$modal', function ($scope, $modalInstance, object, $modal) {

    $scope.itemMasterrr = {};
    $scope.saveData = [];
    if (object) {
        $scope.PanImage = object;
    };

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
}]);
app.controller("ImageControllerAdd", ["$scope", "$modalInstance", "object", '$modal', function ($scope, $modalInstance, object, $modal) {

    $scope.itemMasterrr = {};
    $scope.saveData = [];
    if (object) {
        $scope.AddImage = object;
    };

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
}]);
app.controller("ImageControllerBack", ["$scope", "$modalInstance", "object", '$modal', function ($scope, $modalInstance, object, $modal) {

    $scope.itemMasterrr = {};
    $scope.saveData = [];
    if (object) {
        $scope.BackImage = object;
    };

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
}]);