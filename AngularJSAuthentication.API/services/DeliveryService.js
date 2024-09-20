//'use strict';
//app.factory('DeliveryService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in city ");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var DBoyDeliveryService = {};

//    var _getdboys = function () {

//           return $http.get(serviceBase + 'api/DeliveryOrder').then(function (results) {
//            return results;
//        });

//    };
//    var _getWarehousebyId = function (WarehouseId) {

//        return $http.get(serviceBase + 'api/DeliveryOrder?WarehouseId=' + WarehouseId).then(function (results) {
//            return results;
//        })
//    };

//    DBoyDeliveryService.getWarehousebyId = _getWarehousebyId;

//    var _getordersbyId = function (mob) {
//        return $http.get(serviceBase + 'api/DeliveryOrder?mob=' + mob).then(function (results) {
//            return results;
//        })
//    };

//    DBoyDeliveryService.getordersbyId = _getordersbyId;

//    DBoyDeliveryService.getdboys = _getdboys;
//    var _getDBoyCurrencyID = function (PeopleID) {
//        return $http.get(serviceBase + 'api/CurrencySettle?PeopleID=' + PeopleID).then(function (results) {
//            return results;
//        })
//    };

//    DBoyDeliveryService.getDBoyCurrencyID = _getDBoyCurrencyID;




//    return DBoyDeliveryService;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('DeliveryService', DeliveryService);

    DeliveryService.$inject = ['$http', 'ngAuthSettings'];

    function DeliveryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var DBoyDeliveryService = {};

        var _getdboys = function () {

            return $http.get(serviceBase + 'api/DeliveryOrder').then(function (results) {
                return results;
            });

        };
        DBoyDeliveryService.getdboys = _getdboys;
        var _getWarehousebyId = function (WarehouseId) {
            return $http.get(serviceBase + 'api/DeliveryOrder?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            })
        };

        DBoyDeliveryService.getWarehousebyId = _getWarehousebyId;





        var _getDBoyWarehousebyId = function (WarehouseId) {
            debugger;
            return $http.get(serviceBase + 'api/DeliveryOrder/GetDboy?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            })
        };

        DBoyDeliveryService.getDBoyWarehousebyId = _getDBoyWarehousebyId;

        var _getDBoyIsActiveVicebyId = function (WarehouseId, Active) {
            debugger;
            return $http.get(serviceBase + 'api/DeliveryOrder/GetDboyActiveInActiveViceBasedFilter?WarehouseId=' + WarehouseId + "&Active=" + Active).then(function (results) {
                return results;
            })
        };

        DBoyDeliveryService.getDBoyIsActiveVicebyId = _getDBoyIsActiveVicebyId;

        var _getordersbyId = function (mob) {
            return $http.get(serviceBase + 'api/DeliveryOrder?mob=' + mob).then(function (results) {
                return results;
            })
        };

        DBoyDeliveryService.getordersbyId = _getordersbyId;

        var _getordersbyIdOrdertype = function (mob, OrderType) {
            return $http.get(serviceBase + 'api/DeliveryOrder/ByOrdertype?mob=' + mob + "&OrderType=" + OrderType).then(function (results) {
                return results;
            });
        };

        DBoyDeliveryService.getordersbyIdOrdertype = _getordersbyIdOrdertype;


        DBoyDeliveryService.getdboys = _getdboys;
        var _getDBoyCurrencyID = function (PeopleID) {
            return $http.get(serviceBase + 'api/CurrencySettle?PeopleID=' + PeopleID).then(function (results) {
                return results;
            })
        };

        DBoyDeliveryService.getDBoyCurrencyID = _getDBoyCurrencyID;

        return DBoyDeliveryService;
    }
})();