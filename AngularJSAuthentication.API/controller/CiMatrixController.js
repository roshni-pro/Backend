

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CiMatrixController', CiMatrixController);

    CiMatrixController.$inject = ['$scope', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function CiMatrixController($scope, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            console.log("CiMatrix Start");

            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });

            $scope.Search = function (data) {

                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();

                if (!$('#dat').val() && $scope.srch == "") {
                    start = null;
                    end = null;
                    alert("Please select one parameter");
                    return;
                }
                $scope.Data = [];
                var url = serviceBase + "api/CiMatrix?start=" + start + "&end=" + end;
                $http.get(url).success(function (response) {
                    $scope.Data = response;

                    if ($scope.Data != null) {
                        alasql.fn.myfmt = function (n) {
                            return Number(n).toFixed(2);
                        }
                        alasql('SELECT ItemId,SellingUnitName,Price,CustomerId INTO XLSX("CIMatrixData.xlsx",{headers:true}) FROM ?', [$scope.Data]);
                    }
                    else {
                        alert("No Data Found");
                    }
                });
            };

        }
       
    }
})();