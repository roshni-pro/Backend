(function () {
    'use strict';

    angular
        .module('app')
        .factory('InventoryPageService', InventoryPageService);

    InventoryPageService.$inject = ['$http', 'ngAuthSettings'];

    function InventoryPageService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var InventoryPageServiceFactory = {};

        //var _getinventory = function (StockId) {
        //    
        //    return $http.get(serviceBase + 'api/InventoryEditController/Get?StockId=' + StockId).then(function (results) {
        //        return results;
        //    });
        //};
        //InventoryPageServiceFactory.getinventory = _getinventory;

        var _GetInventoryWarehousebased = function (postData) {
            return $http.post(serviceBase + 'api/InventoryEditController/GetWarehousebased' , postData).then(function (results) {
                return results;
            });
        };
        InventoryPageServiceFactory.GetInventoryWarehousebased = _GetInventoryWarehousebased;

        return InventoryPageServiceFactory;
    }

})();