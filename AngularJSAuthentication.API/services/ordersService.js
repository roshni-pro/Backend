//'use strict';
//app.factory('ordersService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;
//    var ordersServiceFactory = {};
//    var _getOrders = function () {
//        return $http.get(serviceBase + 'api/orders').then(function (results) {
//            return results;
//        });
//    };
//    ordersServiceFactory.getOrders = _getOrders;
//    var _getpriority = function (assignedorders, mobileData) {
        
//        console.log(assignedorders);
//        var data = assignedorders;
//        //var url = serviceBase + 'api/OrderMaster/priority?assignedorders=' + assignedorders + '&Mobile=' + mobileData;
//        return $http.get(serviceBase + 'api/OrderMaster/priority?assignedorders=' + data + '&Mobile=' + mobileData).then(function (results) {
//            return results;
//        });
//    };
//    ordersServiceFactory.getpriority = _getpriority;
//    return ordersServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('ordersService', ordersService);

    ordersService.$inject = ['$http', 'ngAuthSettings'];

    function ordersService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var ordersServiceFactory = {};
        var _getOrders = function () {
            return $http.get(serviceBase + 'api/orders').then(function (results) {
                return results;
            });
        };
        ordersServiceFactory.getOrders = _getOrders;
        var _getpriority = function (assignedorders, mobileData) {

            console.log(assignedorders);
            var data = assignedorders;
            //var url = serviceBase + 'api/OrderMaster/priority?assignedorders=' + assignedorders + '&Mobile=' + mobileData;
            return $http.get(serviceBase + 'api/OrderMaster/priority?assignedorders=' + data + '&Mobile=' + mobileData).then(function (results) {
                return results;
            });
        };
        ordersServiceFactory.getpriority = _getpriority;
        return ordersServiceFactory;
    }
})();