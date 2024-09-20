//'use strict';
//app.factory('projectsService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var projectsServiceFactory = {};

//    var _getprojects = function () {

//        return $http.get(serviceBase + 'api/Projects?type=all').then(function (results) {
//            return results;
//        });
//    };

//    var _getactiveprojects = function () {

//        return $http.get(serviceBase + 'api/Projects?type=active').then(function (results) {
//            return results;
//        });
//    };
//    projectsServiceFactory.getprojects = _getprojects;



//    var _putprojects = function () {

//        return $http.put(serviceBase + 'api/projects').then(function (results) {
//            return results;
//        });
//    };

//    projectsServiceFactory.putprojects = _putprojects;




//    var _deleteprojects = function (data) {
//        console.log("Delete Calling");
//        console.log(data.ProjectID);


//        return $http.delete(serviceBase + 'api/Projects/?id=' + data.ProjectID).then(function (results) {
//            return results;
//        });
//    };

//    projectsServiceFactory.deleteprojects = _deleteprojects;
//    projectsServiceFactory.getactiveprojects = _getactiveprojects;
//    projectsServiceFactory.getprojects = _getprojects;





//    return projectsServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('projectsService', projectsService);

    projectsService.$inject = ['$http', 'ngAuthSettings'];

    function projectsService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var projectsServiceFactory = {};

        var _getprojects = function () {

            return $http.get(serviceBase + 'api/Projects?type=all').then(function (results) {
                return results;
            });
        };

        var _getactiveprojects = function () {

            return $http.get(serviceBase + 'api/Projects?type=active').then(function (results) {
                return results;
            });
        };
        projectsServiceFactory.getprojects = _getprojects;



        var _putprojects = function () {

            return $http.put(serviceBase + 'api/projects').then(function (results) {
                return results;
            });
        };

        projectsServiceFactory.putprojects = _putprojects;




        var _deleteprojects = function (data) {
            console.log("Delete Calling");
            console.log(data.ProjectID);


            return $http.delete(serviceBase + 'api/Projects/?id=' + data.ProjectID).then(function (results) {
                return results;
            });
        };

        projectsServiceFactory.deleteprojects = _deleteprojects;
        projectsServiceFactory.getactiveprojects = _getactiveprojects;
        projectsServiceFactory.getprojects = _getprojects;





        return projectsServiceFactory;
    }
})();