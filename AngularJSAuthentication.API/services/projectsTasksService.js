//'use strict';
//app.factory('projectsTasksService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var tasksServiceFactory = {};

//    var _gettasks = function () {

//        return $http.get(serviceBase + 'api/ProjectTask').then(function (results) {
//            return results;
//        });
//    };

//    tasksServiceFactory.gettasks = _gettasks;



//    var _puttasks = function () {

//        return $http.put(serviceBase + 'api/ProjectTask').then(function (results) {
//            return results;
//        });
//    };

//    tasksServiceFactory.puttasks = _puttasks;




//    var _deletetasks = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Id);


//        return $http.delete(serviceBase + 'api/ProjectTask/?id=' + data.Id).then(function (results) {
//            return results;
//        });
//    };

//    tasksServiceFactory.deletetasks = _deletetasks;
//    tasksServiceFactory.gettasks = _gettasks;





//    return tasksServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('projectsTasksService', projectsTasksService);

    projectsTasksService.$inject = ['$http', 'ngAuthSettings'];

    function projectsTasksService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var tasksServiceFactory = {};

        var _gettasks = function () {

            return $http.get(serviceBase + 'api/ProjectTask').then(function (results) {
                return results;
            });
        };

        tasksServiceFactory.gettasks = _gettasks;



        var _puttasks = function () {

            return $http.put(serviceBase + 'api/ProjectTask').then(function (results) {
                return results;
            });
        };

        tasksServiceFactory.puttasks = _puttasks;




        var _deletetasks = function (data) {
            console.log("Delete Calling");
            console.log(data.Id);


            return $http.delete(serviceBase + 'api/ProjectTask/?id=' + data.Id).then(function (results) {
                return results;
            });
        };

        tasksServiceFactory.deletetasks = _deletetasks;
        tasksServiceFactory.gettasks = _gettasks;





        return tasksServiceFactory;
    }
})();