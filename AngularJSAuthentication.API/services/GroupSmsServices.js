//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .factory('GroupSmsService', GroupSmsService);

//    GroupSmsService.$inject = ['$http', 'ngAuthSettings'];

//    function GroupSmsService($http, ngAuthSettings) {
//        var serviceBase = ngAuthSettings.apiServiceBaseUri;

//        var GroupServiceFactory = {};

//        var _getgroup = function () {
//            //
//            console.log("in area service")
//            return $http.get(serviceBase + 'api/GroupSMS/all').then(function (results) {
//                return results;
//            });
//        };
//        GroupServiceFactory.getgroup = _getgroup;

//        var _getgroupby = function (GroupAssociation) {
//            //
//            console.log("in area service")
//            return $http.get(serviceBase + 'api/GroupSMS/all?GroupAssociation=' + GroupAssociation).then(function (results) {
//                return results;
//            });
//        };
//        GroupServiceFactory.getgroupby = _getgroupby;

//        var _deletegroup = function (data) {


//            return $http.delete(serviceBase + 'api/GroupSMS/delete?id=' + data.GroupID).then(function (results) {
//                return results;
//            });
//        };

//        GroupServiceFactory.deletegroup = _deletegroup;

//        return GroupServiceFactory;
//    }
//})();

(function () {
    'use strict';

    angular
        .module('app')
        .factory('GroupSmsService', GroupSmsService);

    GroupSmsService.$inject = ['$http', 'ngAuthSettings'];

    function GroupSmsService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var GroupServiceFactory = {};

        var _getgroup = function () {
            //
            console.log("in area service")
            return $http.get(serviceBase + 'api/GroupSMS/all').then(function (results) {
                return results;
            });
        };
        GroupServiceFactory.getgroup = _getgroup;

        var _getgroupby = function (GroupAssociation) {
            //
            console.log("in area service")
            return $http.get(serviceBase + 'api/GroupSMS/all?GroupAssociation=' + GroupAssociation).then(function (results) {
                return results;
            });
        };
        GroupServiceFactory.getgroupby = _getgroupby;

        var _deletegroup = function (data) {


            return $http.delete(serviceBase + 'api/GroupSMS/delete?id=' + data.GroupID).then(function (results) {
                return results;
            });
        };

        GroupServiceFactory.deletegroup = _deletegroup;

        return GroupServiceFactory;
    }
})();