
(function () {
    'use strict';

    angular
        .module('app')
        .factory('FreeStockService', FreeStockService);

    FreeStockService.$inject = ['$http', 'ngAuthSettings'];

    function FreeStockService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var StockServiceFactory = {};

      
        var _getstockWarehousebased = function (WarehouseId) {
         
            console.log("get FreeStockService ");

            return $http.get(serviceBase + 'api/freestocks/GetWarehouseFreeStock?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };

        StockServiceFactory.getstockWarehousebased = _getstockWarehousebased;

 


        return StockServiceFactory;
    }
})();