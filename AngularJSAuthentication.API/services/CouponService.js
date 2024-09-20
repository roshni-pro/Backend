//'use strict';
//app.factory('CouponService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var CouponServiceFactory = {};

//    var _getcoupons = function () {

//        return $http.get(serviceBase + 'api/Coupon').then(function (results) {
//            return results;
//        });
//    };

//    CouponServiceFactory.getcoupons = _getcoupons;



//    var _putcoupons = function () {

//        return $http.put(serviceBase + 'api/Coupon').then(function (results) {
//            return results;
//        });
//    };

//    CouponServiceFactory.putcoupons = _putcoupons;




//    var _deleteCoupons = function (data) {

//        return $http.delete(serviceBase + 'api/Coupon/?id=' + data.OfferId).then(function (results) {
//            return results;
//        });
//    };

//    CouponServiceFactory.deleteCoupons = _deleteCoupons;

//    return CouponServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('CouponService', CouponService);

    CouponService.$inject = ['$http', 'ngAuthSettings'];

    function CouponService($http, ngAuthSettings) {
        this.apiServiceBaseUri = '' || this.apiServiceBaseUri;
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var CouponServiceFactory = {};

        var _getcoupons = function () {

            return $http.get(serviceBase + 'api/Coupon').then(function (results) {
                return results;
            });
        };

        CouponServiceFactory.getcoupons = _getcoupons;



        var _putcoupons = function () {

            return $http.put(serviceBase + 'api/Coupon').then(function (results) {
                return results;
            });
        };

        CouponServiceFactory.putcoupons = _putcoupons;




        var _deleteCoupons = function (data) {

            return $http.delete(serviceBase + 'api/Coupon/?id=' + data.OfferId).then(function (results) {
                return results;
            });
        };

        CouponServiceFactory.deleteCoupons = _deleteCoupons;

        return CouponServiceFactory;
    }
})();