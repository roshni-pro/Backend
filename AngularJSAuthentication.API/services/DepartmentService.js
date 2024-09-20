//'use strict';
//app.factory('DepartmentService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in DepartmentService ");
    
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var DepartmentServiceFactory = {};

//    var _getdepartments = function () {
        
//        console.log("get Department");
//        return $http.get(serviceBase + 'api/Department/GetDepartment').then(function (results) {
//            return results;
//        });
//    };

//    DepartmentServiceFactory.getdepartments = _getdepartments;



//    var _putdepartments = function () {

//        return $http.put(serviceBase + 'api/City').then(function (results) {
//            return results;
//        });
//    };

 
//    DepartmentServiceFactory.getdepartments = _getdepartments;


//    return DepartmentServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('DepartmentService', DepartmentService);

    DepartmentService.$inject = ['$http', 'ngAuthSettings'];

    function DepartmentService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var DepartmentServiceFactory = {};

        var _getdepartments = function () {

            console.log("get Department");
            return $http.get(serviceBase + 'api/Department/GetDepartment').then(function (results) {
                return results;
            });
        };

        DepartmentServiceFactory.getdepartments = _getdepartments;



        var _putdepartments = function () {

            return $http.put(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };


        DepartmentServiceFactory.getdepartments = _getdepartments;


        return DepartmentServiceFactory;
    }
})();