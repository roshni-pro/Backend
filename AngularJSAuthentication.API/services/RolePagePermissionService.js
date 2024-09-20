//'use strict';
//app.factory('RolePagePermissionService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in role service");
   
//   var serviceBase = ngAuthSettings.apiServiceBaseUri;
   
//    var RolePagePermissionServiceFactory = {};

//    var _getRole = function () {
//        
//        return $http.get(serviceBase + 'api/RolePagePermission/GetAllRole').then(function (results) {
//            return results;
//        });
//    };

//    RolePagePermissionServiceFactory.getRole = _getRole;

//    return RolePagePermissionServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('RolePagePermissionService', RolePagePermissionService);

    RolePagePermissionService.$inject = ['$http', 'ngAuthSettings'];

    function RolePagePermissionService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var RolePagePermissionServiceFactory = {};

        var _getRole = function () {
            
            return $http.get(serviceBase + 'api/RolePagePermission/GetAllRole').then(function (results) {
                return results;
            });
        };

        RolePagePermissionServiceFactory.getRole = _getRole;

        return RolePagePermissionServiceFactory;
    }
})();