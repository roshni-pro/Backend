//'use strict';
//app.factory('customerService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var customersServiceFactory = {};

//    var _getcustomers = function () {

//        return $http.get(serviceBase + 'api/customers').then(function (results) {
//            return results;
//        });
//    };
//    customersServiceFactory.getcustomers = _getcustomers;
    
//    var _getcustomerBySkcode = function (skcode) {
        
//        return $http.get(serviceBase + 'api/wallet/customer?skcode=' + skcode).then(function (results) {
//            return results;
//        });
//    };
//    customersServiceFactory.getcustomerBySkcode = _getcustomerBySkcode;

//    var _getfiltereddetails = function (data) {
//        return $http.get(serviceBase + 'api/customers?Cityid=' + data.Cityid + '&&' + 'mobile=' + data.mobile + '&&' + 'skcode=' + data.skcode + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
//            return results;
//        });
//    };
//    customersServiceFactory.getfiltereddetails = _getfiltereddetails;

//    var _getfilteredCustomer = function (data) {
        
//        return $http.get(serviceBase + 'api/customers/serach?key=' + data.key).then(function (results) {
//            return results;
//        });
//    };
//    customersServiceFactory.getfilteredCustomer = _getfilteredCustomer;

//    var _putcustomers = function () {

//        return $http.put(serviceBase + 'api/customers').then(function (results) {
//            return results;
//        });
//    };
//    customersServiceFactory.putcustomers = _putcustomers;

//    //missingCustomer
//    var getcustomersMissingDetail = function () {
        
//        return $http.get(serviceBase + 'api/customers/getcustomers').then(function (results) {
//            return results;
//        });
//    };
//    customersServiceFactory.getcustomersMissingDetail = getcustomersMissingDetail;
//    //end

//    var _deletecustomers = function (data) {
        
//        return $http.delete(serviceBase + 'api/customers/?id=' + data.CustomerId).then(function (results) {
//            return results;
//        });
//    };
//    customersServiceFactory.deletecustomers = _deletecustomers;

//    customersServiceFactory.getcustomers = _getcustomers;
//    return customersServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('customerService', customerService);

    customerService.$inject = ['$http', 'ngAuthSettings'];

    function customerService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var customersServiceFactory = {};

        var _getcustomers = function () {

            return $http.get(serviceBase + 'api/customers').then(function (results) {
                return results;
            });
        };
        customersServiceFactory.getcustomers = _getcustomers;

        var _getcustomerBySkcode = function (skcode) {

            return $http.get(serviceBase + 'api/wallet/customer?skcode=' + skcode).then(function (results) {
                return results;
            });
        };
        customersServiceFactory.getcustomerBySkcode = _getcustomerBySkcode;

        var _getfiltereddetails = function (data) {
            return $http.get(serviceBase + 'api/customers?Cityid=' + data.Cityid + '&&' + 'mobile=' + data.mobile + '&&' + 'skcode=' + data.skcode + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
                return results;
            });
        };
        customersServiceFactory.getfiltereddetails = _getfiltereddetails;

        var _getfilteredCustomer = function (data) {

            return $http.get(serviceBase + 'api/customers/serach?key=' + data.key).then(function (results) {
                return results;
            });
        };
        customersServiceFactory.getfilteredCustomer = _getfilteredCustomer;

        var _putcustomers = function () {

            return $http.put(serviceBase + 'api/customers').then(function (results) {
                return results;
            });
        };
        customersServiceFactory.putcustomers = _putcustomers;

        //missingCustomer
        var getcustomersMissingDetail = function () {

            return $http.get(serviceBase + 'api/customers/getcustomers').then(function (results) {
                return results;
            });
        };
        customersServiceFactory.getcustomersMissingDetail = getcustomersMissingDetail;
        //end

        var _deletecustomers = function (data) {

            return $http.delete(serviceBase + 'api/customers/?id=' + data.CustomerId).then(function (results) {
                return results;
            });
        };
        customersServiceFactory.deletecustomers = _deletecustomers;

        customersServiceFactory.getcustomers = _getcustomers;
        return customersServiceFactory;
    }
})();