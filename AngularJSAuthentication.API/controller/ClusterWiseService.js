

(function () {
    'use strict';

    angular
        .module('app')
        .factory('ClusterWiseService', ClusterWiseService);

    ClusterWiseService.$inject = ['$http', 'ngAuthSettings'];

    function ClusterWiseService($http, ngAuthSettings) {
        console.log("in  service");
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var ClusterWiseServiceFactory = {};

        var _getcitys = function () {
            return $http.get(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };
        ClusterWiseServiceFactory.getcitys = _getcitys;

        var _getwarehouse = function () {
            console.log("in warehouse service")
            return $http.get(serviceBase + 'api/Warehouse').then(function (results) {
                return results;
            });
        };
        ClusterWiseServiceFactory.getwarehouse = _getwarehouse;

        var _getclusters = function () {
            console.log("in cluster service")
            return $http.get(serviceBase + 'api/cluster/all').then(function (results) {
                return results;
            });
        };
        ClusterWiseServiceFactory.getclusters = _getclusters;

        return ClusterWiseServiceFactory;

    }
})();