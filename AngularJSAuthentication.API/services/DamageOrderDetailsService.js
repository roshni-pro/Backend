//'use strict';
//app.factory('DamageOrderDetailsService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("details");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var OrderDetailsServiceFactory = {};

  
//    var _getdetails = function () {
//        console.log("serviceeee");
//        //alert("hiiiiii");
//        return $http.get(serviceBase + 'api/DamageOrderDetails?recordtype=details').then(function (results) {
//            return results;
//        });
//    };

//    OrderDetailsServiceFactory.getdetails = _getdetails;

//    var _getallorderdetails = function (i) {
//        return $http.get(serviceBase + 'api/DamageOrderDetails/?id=' + i).then(function (results) {
//            console.log("serve");
//            return results;
//        });
//    };

//    OrderDetailsServiceFactory.getallorderdetails = _getallorderdetails;


//    return OrderDetailsServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('DamageOrderDetailsService', DamageOrderDetailsService);

    DamageOrderDetailsService.$inject = ['$http', 'ngAuthSettings'];

    function DamageOrderDetailsService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var OrderDetailsServiceFactory = {};


        var _getdetails = function () {
            console.log("serviceeee");
            //alert("hiiiiii");
            return $http.get(serviceBase + 'api/DamageOrderDetails?recordtype=details').then(function (results) {
                return results;
            });
        };

        OrderDetailsServiceFactory.getdetails = _getdetails;

        var _getallorderdetails = function (i) {
            return $http.get(serviceBase + 'api/DamageOrderDetails/?id=' + i).then(function (results) {
                console.log("serve");
                return results;
            });
        };

        OrderDetailsServiceFactory.getallorderdetails = _getallorderdetails;


        return OrderDetailsServiceFactory;
    }
})();