
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PointInOutController', PointInOutController);

    PointInOutController.$inject = ['$scope', "WarehouseService", "$filter", "$http", "ngTableParams", '$modal'];

    function PointInOutController($scope, WarehouseService, $filter, $http, ngTableParams, $modal) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

       {

            $scope.compid = UserRole.compid;


            $scope.getWarehosues = function () {
                WarehouseService.getwarehouse().then(function (results) {
                    $scope.warehouse = results.data;
                   
                }, function (error) {
                })
            };

            $(function () {
                
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A',
                });
                $('.input-group-addon').click(function () {
                    $('input[name="daterange"]').trigger("select");
                    //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

                });
            });
            $scope.WarehouseId = "";
            $scope.GetData = function () {
                

                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();

                if (!$('#dat').val() && $scope.WarehouseId == "") {
                    start = null;
                    end = null;
                    alert("Please select one parameter");
                    return;
                }
                $scope.rewardData = [];
                var url = serviceBase + "api/PointInOut?start=" + start + "&end=" + end + "&WarehouseId=" + $scope.WarehouseId;
                $http.get(url).success(function (response) {
                    
                    $scope.rewardData = response;
                })
                    .error(function (data) {
                    })
            }




            //............................Exel export Method.....................//

            alasql.fn.myfmt = function (n) {
                return Number(n).toFixed(2);
            }
            $scope.exportData1 = function () {

                alasql('SELECT OrderId,ShopName,Skcode,TotalAmount,walletPointUsed,WalletAmount,RewardPoint INTO XLSX("CustomerOrderPoint.xlsx",{headers:true}) FROM ?', [$scope.rewardData]);
            }
            //............................Exel export Method.....................//
        }
    }
})();


