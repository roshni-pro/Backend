app.controller('OrderReportingController', ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal', 'DeliveryService', 'localStorageService', function ($scope, WarehouseService, $filter, $http, ngTableParams, $modal, DeliveryService, localStorageService) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
    });

    //$scope.agetWarehosues = function () {

    //    WarehouseService.getwarehouse().then(function (results) {
    //        $scope.warehouse = results.data;
    //        $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
    //        // $scope.AgetOPReport($scope.Warehouseid);
    //    }, function (error) {
    //    })
    //};
    //$scope.agetWarehosues();


    $scope.agetWarehosues = function () {
        var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
        $http.get(url)
            .success(function (data) {
                $scope.warehouse = data;
                $scope.Warehouseid = $scope.warehouse[0].value;
            });

    };
    $scope.agetWarehosues();




    $scope.searchdata = {};
    $scope.searchdata.WarehouseId = 1;
    $scope.searchdata.Mobile = "";

    $scope.DBoys = [];
    $scope.getoldordersdata = function (WarehouseId) {

        DeliveryService.getWarehousebyId(WarehouseId).then(function (resultsdboy) {
            $scope.DBoys = resultsdboy.data;
        }, function (error) {
        });
    };


    //(by neha : 11/09/2019 -date range )
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,

            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'DD/MM/YYYY'
       
        });

        $('.input-group-addon').click(function () {
            $('input[name="daterange"]').trigger("select");
            

        });
        //$('input[name="date"]').on('apply.daterangepicker', function (ev, picker) {
        //    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
        //});
        //$('input[name="date"]').on('cancel.daterangepicker', function (ev, picker) {
        //    $(this).val('');
        //});

    });






    $scope.getoldorders = function (Searchdata) {

        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = f.val();
        var end = g.val();

        if (!Searchdata.WarehouseId || Searchdata.WarehouseId == "" || Searchdata.WarehouseId == 0) {
            alert("Please select warehouse.");
            return false;
        }

        if (!start || !end || start == "" || end == "") {
            alert("Please select Date Range.");
            return false;
        }      

        var url = serviceBase + "api/OrderReporting/GetDboyOrderDeliveryData?dBoyMobileNo=" + Searchdata.Mobile + "&warehouseid=" + Searchdata.WarehouseId + "&fromDate=" + start + "&toDate=" + end;

        $http.get(url)
            .success(function (results) {

                $scope.searchdatadata = results;
            })
            .error(function (data) {
                console.log(data);
            });
    };

}]);
