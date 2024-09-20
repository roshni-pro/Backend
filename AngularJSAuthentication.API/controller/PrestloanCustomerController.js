
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PrestloanCustomerController', PrestloanCustomerController);

    PrestloanCustomerController.$inject = ['$scope', "$filter", "$http", "ngTableParams", 'customerService', 'FileUploader', '$modal', '$log'];

    function PrestloanCustomerController($scope, $filter, $http, ngTableParams, customerService, FileUploader, $modal, $log) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            $scope.PrestaCustomerLoan = function () {


                var url = serviceBase + "api/Prestloanapidata/Loan";
                $http.get(url)
                    .success(function (results) {

                        var RemoveFirstslash = JSON.parse(results);
                        var RemoveSecondslash = JSON.parse(RemoveFirstslash);
                        $scope.LoanData = RemoveSecondslash;

                    })
                    .error(function (data) {
                        console.log(data);
                    })
            };
            $scope.PrestaCustomerLoan();


            $scope.customers = [];
            $scope.getallcustomers = function () {
                customerService.getcustomers().then(function (results) {

                    $scope.customers = results.data;
                }, function (error) {
                });
            }
            $scope.getallcustomers();

        }
       
    }
})();


