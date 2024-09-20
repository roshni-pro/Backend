
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PDCADetailsController', PDCADetailsController);

    PDCADetailsController.$inject = ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal'];

    function PDCADetailsController($scope, WarehouseService, $filter, $http, ngTableParams, $modal) {

        // function for get datetime from date-picker
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });

        $scope.GetAllHistoryData = function (BaseCategoryid, WarehouseId, monthyear) {

            var firstdate = new Date(monthyear).toLocaleDateString();

            var url = serviceBase + 'api/PDCA/GetAllHistoryData?basecategoryid=' + BaseCategoryid + '&warehouseid=' + WarehouseId + '&selectmonth=' + firstdate;
            $http.get(url).success(function (results) {

                $scope.AllHistoryData = results;
                if ($scope.AllHistoryData == null) {
                    alert("Data Not Found");
                }
            })
            $scope.GetPercentage(BaseCategoryid, WarehouseId);
        };

        $scope.Reload = function (BaseCategoryId, WarehouseId, MonthYear) {

            $scope.selectedItemChanged(BaseCategoryId, WarehouseId, MonthYear);
        };

        // function for get PDCA details data according to BaseCategoryid, WarehouseId and date range
        $scope.selectedItemChanged = function (BaseCategoryid, WarehouseId, monthyear) {

            var firstdate = new Date(monthyear).toLocaleDateString();
            var url = serviceBase + 'api/PDCA/GetWarehouseDetailsReport?basecategoryid=' + BaseCategoryid + '&warehouseid=' + WarehouseId + '&selectmonth=' + firstdate;
            $http.get(url)
                .success(function (response) {

                    $scope.Sectiondata = response;
                    $scope.GetTargetAmount(response.warehousereportdetails);
                    $scope.GetAllHistoryData(BaseCategoryid, WarehouseId, monthyear);
                    $scope.GetTargetAmount(responce.warehousereportdetails);
                });
            $scope.GetPercentage(BaseCategoryid, WarehouseId);

        };

        // function for get PDCA details data according to BaseCategoryid, WarehouseId and date range
        $scope.selectedmonth = function (BaseCategoryid, WarehouseId, monthyear) {

            var firstdate = new Date(monthyear).toLocaleDateString();
            var url = serviceBase + 'api/PDCA/GetWarehouseDetailsReport?basecategoryid=' + BaseCategoryid + '&warehouseid=' + WarehouseId + '&selectmonth=' + firstdate;
            $http.get(url)
                .success(function (response) {

                    $scope.PreviewData = response;

                });
            $scope.GetPercentage(BaseCategoryid, WarehouseId);

        };

        // function for Get base category percentage according to BaseCategoryid, WarehouseId
        $scope.GetPercentage = function (BaseCategoryid, WarehouseId) {

            var url = serviceBase + 'api/PDCA/GetPercentage?basecategoryid=' + BaseCategoryid + '&warehouseid=' + WarehouseId;
            $http.get(url)
                .success(function (response) {

                    $scope.Percentage = response;

                });
        };

        // function for calculate days In current Month
        function daysInThisMonth() {
            var now = new Date();
            return new Date(now.getFullYear(), now.getMonth() + 1, 0).getDate();
        }

        // function for calculate current day In current Month
        function currentdate() {
            var now = new Date();
            return new Date(d.getDate());
        }

        //set color for active & inactive
        $scope.set_color = function (data) {
            var days = daysInThisMonth();
            var d = new Date();
            var currentdate = d.getDate();

            var amount = data.TargetAmount / days;
            var mtd = data.MTD / currentdate;
            data.DAmount = amount;
            data.DMTD = mtd;
            data.Difference = mtd - amount;
            if (amount < mtd) {
                return { background: "lightgreen", color: "black" };
            }
            else {
                return { background: "#e94b3b", color: "white" };
            }
        };

        $scope.set_color1 = function (data) {

            var days = daysInThisMonth();
            var d = new Date();
            var currentdate = d.getDate();

            var amount = data.TargetAmount / days;
            var mtd = data.MTD / currentdate;
            var mtdamount = amount * currentdate;
            var mtdtotal = mtd * currentdate;

            data.PresentTarget = mtdamount * 100 / data.TargetAmount;
            if (data.TargetAmount > 0) {
                data.PresentMTD = mtdtotal * 100 / data.TargetAmount;
            }

            var getper = data.PresentMTD - data.PresentTarget;
            data.Percent = getper;
            if (amount < mtd) {
                return { background: "lightgreen", color: "black" };
            }
            else {
                return { background: "#e94b3b", color: "white" };
            }
        };

        $scope.warehouse = {};
        // function for get Warehosues from warehouse service
        $scope.getWarehosues = function () {

            WarehouseService.getwarehousewokpp().then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            });
        };
        $scope.getWarehosues();


        // function for fill base category combo hubwise(warehouse)
        $scope.getBaseCategory = function (WarehouseId) {

            var url = serviceBase + 'api/PDCA/GetBaseCategory?warehouseid=' + WarehouseId;
            $http.get(url)
                .success(function (response) {

                    $scope.BaseCategory = response;
                });

        };

        // function for insert functionality in PDCA detail
        $scope.AddData = function (data, WarehouseId, PDCAAmount, SelectMonth, BaseCategoryId) {
            var month = new Date(SelectMonth).toLocaleDateString();
            var url = serviceBase + 'api/PDCA/AddWarehouseDetails';
            var dtpost = {
                WarehouseReportDetailslist: data,
                WarehouseId: WarehouseId,
                PDCAAmount: PDCAAmount,
                SelectMonth: month,
                BaseCategoryId: BaseCategoryId
            };
            $http.post(url, dtpost)
                .success(function (response) {
                    $scope.BaseCategory = response;
                    response = response.slice(1, -1);
                    alert(response);
                    window.location.reload();
                });

        };
        // function for insert functionality in PDCA Update Target detail
        $scope.UpdateTarget = function (data, WarehouseId, PDCAAmount, SelectMonth, BaseCategoryId) {

            var month = new Date(SelectMonth).toLocaleDateString();
            var url = serviceBase + 'api/PDCA/UpdateWarehouseDetails';
            var dtpost = {
                WarehouseReportDetailslist: data,
                WarehouseId: WarehouseId,
                PDCAAmount: PDCAAmount,
                SelectMonth: month,
                BaseCategoryId: BaseCategoryId
            };
            $http.post(url, dtpost)
                .success(function (response) {
                    $scope.BaseCategory = response;
                    response = response.slice(1, -1);
                    alert(response);
                    window.location.reload();
                });
        };
        // function for fill PercentageAmount according to percentage
        $scope.fillAmount = function (data, CAmount, Percentage) {

            var amount = CAmount * Percentage / 100;

            $scope.BCAmt = amount;
            for (var i = 0; i < data.length; i++) {
                data[i].Amount = amount * data[i].Percentage / 100;
            }
            $scope.GetTargetAmount(data);
        };

        // function for fill target amount according to percentage
        $scope.GetTargetAmount = function (data) {


            var pertotal = 0;
            var peramttotal = 0;
            var premtd = 0; var mtd = 0;
            var percent = 0; var peramt = 0;
            var damt = 0; var dmtd = 0;
            var pretgt = 0; var presentmtd = 0;
            var Total = 0; var Totals = 0;

            for (var i = 0; i < data.length; i++) {
                $scope.set_color(data[i]);
                pertotal = pertotal + data[i].Target;
                if (data[i].Percentage != 0) {
                    data[i].TargetAmount = (data[i].Amount * data[i].Target) / data[i].Percentage;
                }
                peramttotal = peramttotal + data[i].TargetAmount;
                premtd = premtd + data[i].PreviousMTD;
                percent = percent + data[i].Percentage;
                peramt = peramt + data[i].Amount;
                mtd = mtd + data[i].MTD;
                Total = data[i].TargetAmount - data[i].MTD;//Target Amount And MTD Amount
                Totals = Totals + Total;//getting Total Of Target Amount And MTD Amount
                damt = damt + data[i].DAmount;
                dmtd = dmtd + data[i].DMTD;
                $scope.set_color1(data[i]);
                if (data[i].PresentTarget > 0) {
                    pretgt = pretgt + data[i].PresentTarget;
                }

                if (data[i].PresentMTD !== Infinity) {

                    presentmtd = presentmtd + data[i].PresentMTD;
                }
            }
            pretgt = pretgt / data.length;
            presentmtd = presentmtd / data.length;
            $scope.TotalPercentage = pertotal;
            $scope.TotalPerAmt = peramttotal;
            $scope.TotalPercent = percent;
            $scope.TotalPercentAmt = peramt;
            $scope.TotalPreMTD = premtd;
            $scope.TotalMTD = mtd;
            $scope.Totalsdifrance = Totals;
            $scope.TotalDAmount = damt;
            $scope.TotalDMTD = dmtd;
            $scope.TotalPresentTarget = pretgt;
            $scope.TotalPresentMTD = presentmtd;
        };



        // function for get PDCA details data according to date range
        $scope.GetData = function () {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            var url = serviceBase + 'api/PDCA/GetWarehouseReportMonthWise?datefrom=' + start + '&dateto=' + end;
            $http.get(url)
                .success(function (response) {

                    $scope.Sectiondata = response;
                });
        };
    }
})();
