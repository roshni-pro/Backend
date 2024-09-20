//'use strict';
//app.factory('designationservice', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in designation service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var DesignationServiceFactory = {};

//    var _getdesignations = function () {
        

//        return $http.get(serviceBase + 'api/Designation').then(function (results) {
//            return results;
//        });
//    };

//    DesignationServiceFactory.getdesignations = _getdesignations;

//    var _getdesignationbyname = function () {
        

//        return $http.get(serviceBase + 'api/Designation/?name=' + data.DesignationName).then(function (results) {
//            return results;
//        });
//    };

//    DesignationServiceFactory.getdesignationbyname = _getdesignationbyname;

//    var _putdesignations = function () {
        
//        return $http.put(serviceBase + 'api/Designation/cv').then(function (results) {
//            console.log("putdesignations");
//            console.log(results);
//            return results;
//        });
//    };

//    DesignationServiceFactory.putdesignations = _putdesignations;

//    var _deletedesignation = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Designationid);
        

//        return $http.delete(serviceBase + 'api/Designation/?id=' + data.Designationid).then(function (results) {
//            return results;
//        });
//    };

//    DesignationServiceFactory.deletedesignation = _deletedesignation;
//    // DesignationServiceFactory.getdesignations = _getdesignations;
//    //var _postdesignation = function (data) {
//    //    console.log("post Calling");
//    //    console.log(data.Designationid);
//    //    return $http.post(serviceBase + 'api/Designation').then(function (results) {
//    //        console.log(results);
//    //        return results;
//    //    });
//    //};

//    //DesignationServiceFactory.postdesignation = _postdesignation;




//    return DesignationServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('designationservice', designationservice);

    designationservice.$inject = ['$http', 'ngAuthSettings'];

    function designationservice($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var DesignationServiceFactory = {};

        var _getdesignations = function () {


            return $http.get(serviceBase + 'api/Designation').then(function (results) {
                return results;
            });
        };

        DesignationServiceFactory.getdesignations = _getdesignations;

        var _getdesignationbyname = function () {


            return $http.get(serviceBase + 'api/Designation/?name=' + data.DesignationName).then(function (results) {
                return results;
            });
        };

        DesignationServiceFactory.getdesignationbyname = _getdesignationbyname;

        var _putdesignations = function () {

            return $http.put(serviceBase + 'api/Designation/cv').then(function (results) {
                console.log("putdesignations");
                console.log(results);
                return results;
            });
        };

        DesignationServiceFactory.putdesignations = _putdesignations;

        var _deletedesignation = function (data) {
            console.log("Delete Calling");
            console.log(data.Designationid);


            return $http.delete(serviceBase + 'api/Designation/?id=' + data.Designationid).then(function (results) {
                return results;
            });
        };

        DesignationServiceFactory.deletedesignation = _deletedesignation;
        // DesignationServiceFactory.getdesignations = _getdesignations;
        //var _postdesignation = function (data) {
        //    console.log("post Calling");
        //    console.log(data.Designationid);
        //    return $http.post(serviceBase + 'api/Designation').then(function (results) {
        //        console.log(results);
        //        return results;
        //    });
        //};

        //DesignationServiceFactory.postdesignation = _postdesignation;




        return DesignationServiceFactory;
    }
})();