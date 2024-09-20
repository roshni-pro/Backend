
(function () {
    'use strict';

    angular
        .module('app')
        .controller('GDNController', GDNController);

    GDNController.$inject = ['$scope', "$rootScope", "$routeParams", "$filter", "$http", "ngTableParams", 'ngAuthSettings', 'FileUploader', 'authService'];
    function GDNController($scope, $rootScope, $routeParams, $filter, $http, ngTableParams, ngAuthSettings, FileUploader, authService) {
        
        $scope.Valid = false;
        $scope.Poid = $routeParams.id;
        $scope.GrSno = $routeParams.GrSno;

        $scope.GetGDNdata = function (GrSno, Poid) {
            debugger
           // $http.get(serviceBase + 'api/SarthiApp/GetGDNdetails?GDNid=' + id).then(function (results) {
            $http.get(serviceBase + 'api/SarthiApp/GetGDNdetailOnIR?Poid=' + Poid + '&GrSNo=' + GrSno).then(function (results) {
                debugger
                if (results.data.length>0)
                    $scope.GDNData = results.data;
                else 
                    alert('No Data Found')
                 

            });
        };
        $scope.GetGDNdata($scope.GrSno,$scope.Poid);


        $scope.ValidateOtp = function (otp, Poid) {

            $scope.Valid = false;
            $scope.action = false;
            $http.get(serviceBase + 'api/SarthiApp/ValidateOtp?otp=' + otp + '&Poid=' + Poid).then(function (results) {

                if (results.data) {
                    $scope.Valid = true;
                    //$scope.action = true;
                }
                else {
                    alert("Invalid OTP")
                }
            });
        };
        $scope.GdnApprove = function (gndid, Poid, comment) {

           debugger
             
            if (confirm("Are you sure want to Approve ?")) {
                var data = {
                    status: "A",
                    PurchaseOrderId: Poid,
                    GDNId: gndid,
                    Comment: comment
                }
                $http.post(serviceBase + 'api/SarthiApp/GDNAction', data).success(function (response) {
                    if (response) {
                        alert("GDN has been Approved");
                        $scope.action = false;
                    }
                    else {
                        alert("Error: Try after sometime");
                        return false;
                    }
                });
            }
        };
        $scope.GdnReject = function (gdnid, Poid, comment) {

          
            if (confirm("Are you sure want to Reject ?")) {
                var data = {
                    status: "R",
                    PurchaseOrderId: Poid,
                    GDNId: gdnid,
                    Comment: comment
                }
                $http.post(serviceBase + 'api/SarthiApp/GDNAction', data).success(function (response) {
                    if (response) {
                        alert("GDN has been Rejected");
                        $scope.action = false;
                    }
                    else {
                        alert("Error: Try after sometime");
                        return false;
                    }
                });
            }
        };

    }
})();