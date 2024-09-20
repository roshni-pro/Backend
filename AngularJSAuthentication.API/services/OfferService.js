//'use strict';
//app.factory('OfferService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var OfferServiceFactory = {};

//    var _getoffer = function () {

//        return $http.get(serviceBase + 'api/offer').then(function (results) {
//            return results;
//        });
//    };

//    OfferServiceFactory.getoffer = _getoffer;

//    var _getofferBill = function () {
        
//        return $http.get(serviceBase + 'api/offer/Bill').then(function (results) {
//            return results;
//        });
//    };

//    OfferServiceFactory.getofferBill = _getofferBill ;

//    var _putoffer = function () {

//        return $http.put(serviceBase + 'api/offer').then(function (results) {
//            return results;
//        });
//    };

//    OfferServiceFactory.putoffer = _putoffer;




//    var _deleteoffer = function (data) {

//        return $http.delete(serviceBase + 'api/offer/?id=' + data.OfferId).then(function (results) {
//            return results;
//        });
//    };

//    OfferServiceFactory.deleteoffer = _deleteoffer;

//    return OfferServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('OfferService', OfferService);

    OfferService.$inject = ['$http', 'ngAuthSettings'];

    function OfferService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var OfferServiceFactory = {};

        var _getoffer = function () {

            return $http.get(serviceBase + 'api/offer').then(function (results) {
                return results;
            });
        };

        OfferServiceFactory.getoffer = _getoffer;

        var _getofferBill = function () {

            return $http.get(serviceBase + 'api/offer/Bill').then(function (results) {
                return results;
            });
        };

        OfferServiceFactory.getofferBill = _getofferBill;

        var _putoffer = function () {

            return $http.put(serviceBase + 'api/offer').then(function (results) {
                return results;
            });
        };

        OfferServiceFactory.putoffer = _putoffer;




        var _deleteoffer = function (data) {

            return $http.delete(serviceBase + 'api/offer/?id=' + data.OfferId).then(function (results) {
                return results;
            });
        };

        OfferServiceFactory.deleteoffer = _deleteoffer;

        return OfferServiceFactory;
    }
})();