//'use strict';
//app.factory('RBLCustomerInfoService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;


//    var RBLCustomerInfoServiceFactory = {};


//    var _getRBLCustomerInfo = function () {
//        console.log("calling get rblcustomer");
//        return $http.get(serviceBase + 'api/RBLCustomerInformation/GetRBLCustInformation').then(function (results) {
//            return results;
//        });
//    };
//    RBLCustomerInfoServiceFactory.getRBLCustomerInfo = _getRBLCustomerInfo1;
//    var _getRBLCustomerInfo1 = function () {

//        return $http.get(serviceBase + 'api/RBLCustomerInformation/rbldata').then(function (results) {
//            return results;
//        });
//    };
//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('RBLCustomerInfoService', RBLCustomerInfoService);

    RBLCustomerInfoService.$inject = ['$http', 'ngAuthSettings'];

    function RBLCustomerInfoService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;


        var RBLCustomerInfoServiceFactory = {};


        var _getRBLCustomerInfo = function () {
            console.log("calling get rblcustomer");
            return $http.get(serviceBase + 'api/RBLCustomerInformation/GetRBLCustInformation').then(function (results) {
                return results;
            });
        };
        RBLCustomerInfoServiceFactory.getRBLCustomerInfo = _getRBLCustomerInfo1;
        var _getRBLCustomerInfo1 = function () {

            return $http.get(serviceBase + 'api/RBLCustomerInformation/rbldata').then(function (results) {
                return results;
    }
})();