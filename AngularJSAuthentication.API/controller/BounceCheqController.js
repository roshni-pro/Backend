

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BounceCheqController', BounceCheqController);

    BounceCheqController.$inject = ['$scope', "$filter", "$http", "ngTableParams", "SalesService"];

    function BounceCheqController($scope, $filter, $http, ngTableParams, SalesService) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
       {
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

            $scope.currentPageStores = {};
            $scope.pageno = 1; // initialize page no to 1
            $scope.total_count = 0;
            $scope.itemsPerPage = 50; //this could be a dynamic value from a drop down
            $scope.numPerPageOpt = [50, 100, 200, 300];//dropdown options for no. of Items per page


            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;
                $scope.getBouncedata($scope.pageno);
            }
            $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown

            $scope.$on('$viewContentLoaded', function () {
                $scope.getBouncedata($scope.pageno);
            });

            $scope.getBouncedata = function (pageno) {
                $scope.currentPageStores = {};
                var url = serviceBase + "api/bouncecheq/saless" + "?list=" + $scope.itemsPerPage + "&page=" + pageno;

                $http.get(url)
                    .success(function (results) {
                        $scope.currentPageStores = results.ordermaster;
                        $scope.total_count = results.total_count;
                    })
                    .error(function (data) {
                        console.log(data);
                    })
            };


            $scope.cheque = function (data) {
                var url = serviceBase + "api/bouncecheq/Bounce";
                $http.put(url, data)
                    .then(function (response) {
                        $scope.cheque = response.data;
                        location.reload();
                    });
            };




            //var dd = SalesService.getDeatil();
            //console.log("bounce selected")
            //console.log(dd);
            //$scope.d = dd;
            //$scope.CheqBounceupdate = function (data) {
            //    var url = serviceBase + "api/salessettlement/Bounce";
            //    $http.put(url, data).then(function (response) {
            //        $scope.cashbaoun = response.data;
            //        window.location = "#/BounceCheq";

            //    });
            //}




            $scope.srch = "";
            $scope.searchdata = function (data) {

                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();
                $scope.currentPageStores = [];
                var url = serviceBase + "api/bouncecheq/search?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId;
                $http.get(url).success(function (response) {
                    $scope.currentPageStores = response;
                    $scope.total_count = response.length;
                });

            }
        }
        
    }
})();
