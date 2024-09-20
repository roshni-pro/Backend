//'use strict';
//app.factory('ProjectService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in Project service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var ProjectServiceFactory = {};

//    var _getProjects = function () {
        
//        return $http.get(serviceBase + 'api/Projects').then(function (results) {
//            return results;
//        });
//    };
//    ProjectServiceFactory.getProjects = _getProjects;

//    var _putProjects = function () {

//        return $http.put(serviceBase + 'api/Projects').then(function (results) {
//            console.log("putProjects");
//            console.log(results);
//            return results;
//        });
//    };
//    ProjectServiceFactory.putProjects = _putProjects;

//    var _deleteProject = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Projectid);


//        return $http.delete(serviceBase + 'api/Projects/?id=' + data.Projectid).then(function (results) {
//            return results;
//        });
//    };
//    ProjectServiceFactory.deleteProject = _deleteProject;
//    ProjectServiceFactory.getProjects = _getProjects;
    
//    var _getIssueCategorys = function () { // By Sudhir
//        return $http.get(serviceBase + 'api/Projects/GetIssueCategory').then(function (results) {
//            return results;
//        });
//    };
//    ProjectServiceFactory.getIssueCategorys = _getIssueCategorys;

//    return ProjectServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('ProjectService', ProjectService);

    ProjectService.$inject = ['$http', 'ngAuthSettings'];

    function ProjectService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var ProjectServiceFactory = {};

        var _getProjects = function () {

            return $http.get(serviceBase + 'api/Projects').then(function (results) {
                return results;
            });
        };
        ProjectServiceFactory.getProjects = _getProjects;

        var _putProjects = function () {

            return $http.put(serviceBase + 'api/Projects').then(function (results) {
                console.log("putProjects");
                console.log(results);
                return results;
            });
        };
        ProjectServiceFactory.putProjects = _putProjects;

        var _deleteProject = function (data) {
            console.log("Delete Calling");
            console.log(data.Projectid);


            return $http.delete(serviceBase + 'api/Projects/?id=' + data.Projectid).then(function (results) {
                return results;
            });
        };
        ProjectServiceFactory.deleteProject = _deleteProject;
        ProjectServiceFactory.getProjects = _getProjects;

        var _getIssueCategorys = function () { // By Sudhir
            return $http.get(serviceBase + 'api/Projects/GetIssueCategory').then(function (results) {
                return results;
            });
        };
        ProjectServiceFactory.getIssueCategorys = _getIssueCategorys;

        return ProjectServiceFactory;
    }
})();