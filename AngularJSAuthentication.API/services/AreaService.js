//'use strict';
    //app.factory('AreaService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

    //    var serviceBase = ngAuthSettings.apiServiceBaseUri;

    //    var AreaServiceFactory = {};

    //    var _getarea = function () {
    //        console.log("in area service")
    //        return $http.get(serviceBase + 'api/area/all').then(function (results) {
    //            return results;
    //        });
    //    };
    //    AreaServiceFactory.getarea = _getarea;
    //    // get area city id based 
    //    var _getareacityid = function (cityId) {
    //        console.log("in area service")
    //        return $http.get(serviceBase + 'api/area/GetArea?CityId=' + cityId).then(function (results) {
    //            return results;
    //        });
    //    };
    //    AreaServiceFactory.getareacityid = _getareacityid;
    //    var _deletearea = function (data) {
    //        return $http.delete(serviceBase + 'api/area/delete?id=' + data.areaId).then(function (results) {
    //            return results;
    //        });
    //    };

    //    AreaServiceFactory.deletearea = _deletearea;

    //    return AreaServiceFactory;
    //}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('AreaService', AreaService);

    AreaService.$inject = ['$http', 'ngAuthSettings'];

    function AreaService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var AreaServiceFactory = {};

        var _getarea = function () {
            console.log("in area service")
            return $http.get(serviceBase + 'api/area/all').then(function (results) {
                return results;
            });
        };
        AreaServiceFactory.getarea = _getarea;
        // get area city id based 
        var _getareacityid = function (cityId) {
            console.log("in area service")
            return $http.get(serviceBase + 'api/area/GetArea?CityId=' + cityId).then(function (results) {
                return results;
            });
        };
        AreaServiceFactory.getareacityid = _getareacityid;
        var _deletearea = function (data) {
            return $http.delete(serviceBase + 'api/area/delete?id=' + data.areaId).then(function (results) {
                return results;
            });
        };

        AreaServiceFactory.deletearea = _deletearea;

        return AreaServiceFactory;
    }
})();