(function () {
    'use strict';

    angular
        .module('app')
        .controller('DownloadItemLedgerController', DownloadItemLedgerController);

    DownloadItemLedgerController.$inject = ['$scope', "$http", '$modal', 'OrderMasterService', 'ngTableParams', '$routeParams', "localStorageService", "WarehouseService", '$filter'];

    function DownloadItemLedgerController($scope, $http, $modal, OrderMasterService, ngTableParams, $routeParams, localStorageService, WarehouseService, $filter) {
      

        //$(function () {
        //    $('input[name="daterange"]').daterangepicker({
        //        timePicker: true,
        //        timePickerIncrement: 5,
        //        timePicker12Hour: true,
        //        format: 'MM/DD/YYYY h:mm A',
        //    });
        //});

        OrderMasterService.getwarehouse().then(function (results) {
                if (results.data.length > 0) {
                    for (var a = 0; a < results.data.length; a++) {
                        results.data[a].WarehouseName = results.data[a].WarehouseName + " " + results.data[a].CityName;
                    }

                    $scope.warehouse = results.data;

                } else { alert('No record '); }
                //

            }, function (error) {
            });
        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };  




        //(by neha : 11/09/2019 -date range )
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,

                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            
            });

            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                

            });
           

        });
        //by neha 23/08/19
        $scope.SearchData = function (WarehouseId) {
            
            $scope.DownloadItemLedger = {};
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
           
            let whids = $scope.examplemodel.map(a => a.id);
            var url = serviceBase + 'api/ItemLedger?wareHouseIds=' + whids + '&startDate=' + start + '&endDate=' + end;
            $http.get(url)
                .success(function (result) {   
                    console.log('item ledger : ', result);
                    alasql('SELECT ItemName,ItemCode,Brand,Category,WarehouseName,APP,OpeningStock,OpeningStockAmount,PoInwardQty,PoInwardTotalAmount,WareHouseInQty,WarehouseInAmount,WarehouseOutQty,WarehouseOutAmount,SaleQuantity,SaleAmount,pilferageAmount,pilferageQty,CancelInQty,CancelInAmount,SystemClosingQty,SystemClosingAmount,POReturnQty,POReturnAmount,ManualInventoryInQty,ManualInventoryInAmount,ManualInventoryOutQty,ManualInventoryOutAmount,FreeQuantity,TotalInQty,TotalOutQty,InOutDiffQty,InOutDiffAmount,GrossProfit,GrossProfitAfterPilferage,profitability INTO XLSX("DownloadItemLedgerReport.xlsx",{headers:true}) FROM ?', [result]);

                });
        }

       
    }
})();