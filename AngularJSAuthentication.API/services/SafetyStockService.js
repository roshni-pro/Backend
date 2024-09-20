
(function () {
    'use strict';

    angular
        .module('app')
        .factory('SafetyStockService', SafetyStockService);

    SafetyStockService.$inject = ['$http', 'ngAuthSettings'];

    function SafetyStockService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var SafetyStockServiceFactory = {};

        var _getcitys = function () {
            return $http.get(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };
        SafetyStockServiceFactory.getcitys = _getcitys;
        var _getwarehouse = function () {
            
            console.log("in warehouse service")
            return $http.get(serviceBase + 'api/Warehouse/GetHub').then(function (results) {
                return results;
            });
        };
        SafetyStockServiceFactory.getwarehouse = _getwarehouse;
        return SafetyStockServiceFactory;
    }


});