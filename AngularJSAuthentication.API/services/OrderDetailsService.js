//'use strict';
//app.factory('OrderDetailsService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("details");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var OrderDetailsServiceFactory = {};

  
//    var _getdetails = function () {
//        console.log("serviceeee");
//        //alert("hiiiiii");
//        return $http.get(serviceBase + 'api/OrderDetails?recordtype=details').then(function (results) {
//            return results;
//        });
//    };

//    OrderDetailsServiceFactory.getdetails = _getdetails;

//    var _getallorderdetails = function (i) {
//        return $http.get(serviceBase + 'api/OrderDetails/?id='+i).then(function (results) {
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
        .factory('OrderDetailsService', OrderDetailsService);

    OrderDetailsService.$inject = ['$http', 'ngAuthSettings'];

    function OrderDetailsService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var OrderDetailsServiceFactory = {};


        var _getdetails = function () {
            console.log("serviceeee");
            //alert("hiiiiii");
            return $http.get(serviceBase + 'api/OrderDetails?recordtype=details').then(function (results) {
                return results;
            });
        };

        OrderDetailsServiceFactory.getdetails = _getdetails;

        var _getallorderdetails = function (i) {
            return $http.get(serviceBase + 'api/OrderDetails/?id=' + i).then(function (results) {
                console.log("serve");
                return results;
            });
        };

        OrderDetailsServiceFactory.getallorderdetails = _getallorderdetails;


        return OrderDetailsServiceFactory;
    }
})();