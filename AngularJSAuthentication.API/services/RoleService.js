//'use strict';
//app.factory('RoleService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in role service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var RoleServiceFactory = {};

//    var _getRoles = function () {

//        return $http.get(serviceBase + 'api/Roles').then(function (results) {
//            return results;
//        });
//    };

//    RoleServiceFactory.getRoles = _getRoles;



//    var _putroles = function () {

//        return $http.put(serviceBase + 'api/Roles').then(function (results) {
//            console.log("put");
//            console.log(results);
//            return results;
//        });
//    };

//    RoleServiceFactory.putroles = _putroles;




//    var _deleteRole = function (data) {
//        console.log("Delete Calling");
//        console.log(data.RoleId);
//        return $http.delete(serviceBase + 'api/Roles?id=' + data.RoleId).then(function (results) {
//            return results;
//        });
//    };

//    RoleServiceFactory.deleteRole = _deleteRole;
//    RoleServiceFactory.getRoles = _getRoles;





//    return RoleServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('RoleService', RoleService);

    RoleService.$inject = ['$http', 'ngAuthSettings'];

    function RoleService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var RoleServiceFactory = {};

        var _getRoles = function () {

            return $http.get(serviceBase + 'api/Roles').then(function (results) {
                return results;
            });
        };

        RoleServiceFactory.getRoles = _getRoles;



        var _putroles = function () {

            return $http.put(serviceBase + 'api/Roles').then(function (results) {
                console.log("put");
                console.log(results);
                return results;
            });
        };

        RoleServiceFactory.putroles = _putroles;




        var _deleteRole = function (data) {
            console.log("Delete Calling");
            console.log(data.RoleId);
            return $http.delete(serviceBase + 'api/Roles?id=' + data.RoleId).then(function (results) {
                return results;
            });
        };

        RoleServiceFactory.deleteRole = _deleteRole;
        RoleServiceFactory.getRoles = _getRoles;





        return RoleServiceFactory;
    }
})();