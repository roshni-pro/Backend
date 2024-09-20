

(function () {
    'use strict';

    angular
        .module('app')
        .controller('FirstTimeOrderController', FirstTimeOrderController);

    FirstTimeOrderController.$inject = ['$scope', '$http', '$modal', 'WarehouseService', 'CityService'];


    function FirstTimeOrderController($scope, $http, $modal, WarehouseService, CityService) {


         $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'YYYY/MM/DD h:mm A',
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
         });

       
        $scope.actions = [];
        $scope.cities = [];
        CityService.getcitys().then(function (results) {

            $scope.cities = results.data;

        }, function (error) {
        });
        $scope.warehouse = [];
        $scope.getWarehosues = function (cityid) {
            
            $http.get("/api/Warehouse/GetWarehouseCity/?CityId=" + cityid).then(function (results) {
                $scope.warehouse = results.data;

                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;

                //$scope.getData($scope.WarehouseId);
            }, function (error) {
            })
        };
        //$scope.getData = function (WarehouseId) {
        //    $http.get("/api/OrderMaster/getdetail/?Warehouseid=" + WarehouseId).then(function (results) {

        //        $scope.actions = results.data;
        //        console.log($scope.actions);
        //    }, function (error) {
        //    });
        //}



        $scope.getDataRe = function (WarehouseId) {
            
            var start = "";
            var end = "";
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            if (!$('#dat').val()) {
                end = '';
                start = '';
                alert("Select Start and End Date")
                return;
            }
            else {
                start = f.val();
                end = g.val();
            }
            if ($scope.WarehouseId != "" && $scope.WarehouseId != 0) {
                
                $http.get("/api/OrderMaster/getdetail/?Warehouseid=" + WarehouseId + "&start=" + start + "&end=" + end).then(function (results) {

                    $scope.actions = results.data;
                    console.log($scope.actions);
                }, function (error) {
                });
            }
            else { alert("select type") }
        }
            
        









        $scope.firsttimeorder = [];
        $scope.ExportAllDataFirstOrder = function () {

            $scope.firsttimeorder = $scope.actions;
            alasql('SELECT CustomerId,CustomerName,ShopName,Skcode,WarehouseId,WarehouseName,CityName,Mobile,Address,DateOfPurchase,amount INTO XLSX("CustomerDetail.xlsx",{headers:true}) FROM ?', [$scope.firsttimeorder]);

        };
    }
})();


