//'use strict';
//app.factory('leavesService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var leavesServiceFactory = {};



//    var _getleaves = function () {

//        return $http.get(serviceBase + 'api/Leaves').then(function (results) {
//            return results;
//        });
//    };
//    leavesServiceFactory.getleaves = _getleaves;



//    var _putleaves = function () {

//        return $http.put(serviceBase + 'api/Leaves').then(function (results) {
//            return results;
//        });
//    };
//    leavesServiceFactory.putleaves = _putleaves;




//    var _deleteleaves = function (data) {
//        console.log("Delete Calling");
//        console.log(data.LeaveID);
//        return $http.delete(serviceBase + 'api/Leaves/?id=' + data.LeaveID).then(function (results) {
//            return results;
//        });
//    };

//    leavesServiceFactory.deleteleaves = _deleteleaves;
//    leavesServiceFactory.getleaves = _getleaves;
//    return leavesServiceFactory;
//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('leavesService', leavesService);

    leavesService.$inject = ['$http', 'ngAuthSettings'];

    function leavesService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var leavesServiceFactory = {};



        var _getleaves = function () {

            return $http.get(serviceBase + 'api/Leaves').then(function (results) {
                return results;
            });
        };
        leavesServiceFactory.getleaves = _getleaves;



        var _putleaves = function () {

            return $http.put(serviceBase + 'api/Leaves').then(function (results) {
                return results;
            });
        };
        leavesServiceFactory.putleaves = _putleaves;




        var _deleteleaves = function (data) {
            console.log("Delete Calling");
            console.log(data.LeaveID);
            return $http.delete(serviceBase + 'api/Leaves/?id=' + data.LeaveID).then(function (results) {
                return results;
            });
        };

        leavesServiceFactory.deleteleaves = _deleteleaves;
        leavesServiceFactory.getleaves = _getleaves;
        return leavesServiceFactory;
    }
})();