

(function () {
    'use strict';

    angular
        .module('app')
        .controller('RBLCustomerInformationController', RBLCustomerInformationController);

    RBLCustomerInformationController.$inject = ['$scope', "$filter", "$http", "ngTableParams", 'FileUploader', '$modal', '$log'];

    function RBLCustomerInformationController($scope, $filter, $http, ngTableParams, FileUploader, $modal, $log) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'))

        //$scope.Getrbldata = [];
        ////$scope.GetstockParent = [];
        //$scope.GetRBLCustInformation = function () {
        //    
        //    var url = serviceBase + "api/RBLCustomerInformation/GetRBLCustInformation";
        //    $http.get(url).success(function (response) {
        //        
        //        $scope.Get = response; //ajax request to fetch data into vm.data 
        //    });
        //};

        $scope.Getrbldata = [];
        $scope.Getrbldata = function () {

            $http.get(serviceBase + 'api/RBLCustomerInformation/rbldata').then(function (results) {


                $scope.Getrbldata = results.data; //ajax request to fetch data into vm.data 
            });
        };
        $scope.Getrbldata();

    }
})();