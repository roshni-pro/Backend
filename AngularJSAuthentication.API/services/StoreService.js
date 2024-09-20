(function () {
    'use strict';

    angular
        .module('app')
        .factory('StoreService', StoreService);

    StoreService.$inject = ['$http', 'ngAuthSettings'];

    function StoreService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var StoreServiceData = {};
        var _getStore = function () {
            return $http.get(serviceBase + 'api/Store/GetStoreList').then(function (results) {

                return results;
            });
        };
        StoreServiceData.getStore = _getStore;
        return StoreServiceData;
    }
})();