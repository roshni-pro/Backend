
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PDCADataCompairController', PDCADataCompairController);

    PDCADataCompairController.$inject = ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal'];

    function PDCADataCompairController($scope, WarehouseService, $filter, $http, ngTableParams, $modal) {

        // function for get datetime from date-picker
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });

        $scope.warehouse = {};
        $scope.warehouseCategoryDetail = [];
        // function for get Warehosues from warehouse service
        $scope.getWarehosues = function () {
            WarehouseService.getwarehousewokpp().then(function (results) {
                if (results.data.length > 0) {
                    for (var a = 0; a < results.data.length; a++) {
                        results.data[a].WarehouseName = results.data[a].WarehouseName + " " + results.data[a].CityName;
                    }
                    $scope.warehouse = results.data;
                }
            }, function (error) {
            });
        };
        $scope.getWarehosues();

        $scope.GettargetAmount = function (wid, categorywarehouse) {
            var targetamt = 0;
            _.each(categorywarehouse, function (ws) {
                if (ws.WareHouseid == wid) {
                    targetamt = ws.TargetAmount;
                }
            });
            return targetamt;
        };

        $scope.GetMTD = function (wid, categorywarehouse) {
            var MTDamt = 0;
            _.each(categorywarehouse, function (ws) {
                if (ws.WareHouseid == wid) {
                    MTDamt = ws.MTD;
                }
            });
            return MTDamt;
        };

        $scope.SelectedWarehouse = [];
        // function for get PDCA details data according to date range
        $scope.GetData = function (month) {
            var firstdate = new Date(month).toLocaleDateString();
            var ids = [];
            var selectedWarehouse = [];
            $scope.SelectedWarehouse = [];
            _.each($scope.examplemodel, function (o2) {
                ids.push(o2.id);
                for (var i = 0; i < $scope.warehouse.length; i++) {
                    if ($scope.warehouse[i].WarehouseId == o2.id) {
                        selectedWarehouse.push({ "Id": o2.id, "Name": $scope.warehouse[i].WarehouseName });
                    }
                }
            });

            $scope.SelectedWarehouse = selectedWarehouse;

            $scope.Header = [];
            var i = 1;
            $scope.SelectedWarehouse.forEach(function (item) {
                $scope.Header.push(i++);
                $scope.Header.push(i++);
            });
            //window.location.reload();
            var url = serviceBase + 'api/PDCA/GetWarehouseComparisonData?month=' + firstdate;
            var datatopost = ids;
            $http.post(url, datatopost)
                .success(function (response) {
                    $scope.CategoryCompare = response;
                });

        };

        $scope.ExportData = function (month) {
            if (month === undefined) {
                alert('Please select Month');
            }
            else {
                var firstdate = new Date(month).toLocaleDateString();
                var url = serviceBase + 'api/PDCA/ExportData?month=' + firstdate;
                //var datatopost = ids;
                $http.get(url)
                    .success(function (response) {
                        $scope.Category = response;
                        $scope.Export($scope.Category);
                    });
            }
        };

        $scope.Export = function (data) {
            alasql('SELECT CategoryName,BrandName,ItemName,H1Indore,H2Indore,H1Bhopal,H1Jaipur,H2Jaipur,H2Bhopal INTO XLSX("PDCAlist.xlsx",{headers:true}) FROM ?', [data]);
        };


        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };
    }
})();