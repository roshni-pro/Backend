//'use strict';
//app.factory('CustomerIssuesService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var CustomerIssuesServiceFactory = {};

//    var _getcustomerissues = function () {

//        return $http.get(serviceBase + 'api/CustomerIssue/GetAll').then(function (results) {
//            return results;
//        });
//    };

//    CustomerIssuesServiceFactory.getcustomerissues = _getcustomerissues;



//    var _PutCustomerissues = function (data) {

//        return $http.put(serviceBase + 'api/CustomerIssue/Update',data).then(function (results) {
//            return results;
//        });
//    };

//    CustomerIssuesServiceFactory.PutCustomerissues = _PutCustomerissues;


//    var _PostCustomerissue = function (data) {
        
//        return $http.post(serviceBase + 'api/CustomerIssue/Add', data).then(function (results) {
//            return results;
//        });
//    };

//    CustomerIssuesServiceFactory.PostCustomerissue = _PostCustomerissue;


//    var _deletecustomerissues = function (data) {
//        console.log("Delete Calling");
//        console.log(data.CS_id);
//        return $http.delete(serviceBase + 'api/CustomerIssue/Delete?CS_id=' + data.CS_id).then(function (results) {
//            return results;
//        });
//    };

//    CustomerIssuesServiceFactory.deletecustomerissues = _deletecustomerissues;
//    CustomerIssuesServiceFactory.getcustomerissues = _getcustomerissues;
//    return CustomerIssuesServiceFactory;
//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('CustomerIssuesService', CustomerIssuesService);

    CustomerIssuesService.$inject = ['$http', 'ngAuthSettings'];

    function CustomerIssuesService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var CustomerIssuesServiceFactory = {};

        var _getcustomerissues = function () {

            return $http.get(serviceBase + 'api/CustomerIssue/GetAll').then(function (results) {
                return results;
            });
        };

        CustomerIssuesServiceFactory.getcustomerissues = _getcustomerissues;



        var _PutCustomerissues = function (data) {

            return $http.put(serviceBase + 'api/CustomerIssue/Update', data).then(function (results) {
                return results;
            });
        };

        CustomerIssuesServiceFactory.PutCustomerissues = _PutCustomerissues;


        var _PostCustomerissue = function (data) {

            return $http.post(serviceBase + 'api/CustomerIssue/Add', data).then(function (results) {
                return results;
            });
        };

        CustomerIssuesServiceFactory.PostCustomerissue = _PostCustomerissue;


        var _deletecustomerissues = function (data) {
            console.log("Delete Calling");
            console.log(data.CS_id);
            return $http.delete(serviceBase + 'api/CustomerIssue/Delete?CS_id=' + data.CS_id).then(function (results) {
                return results;
            });
        };

        CustomerIssuesServiceFactory.deletecustomerissues = _deletecustomerissues;
        CustomerIssuesServiceFactory.getcustomerissues = _getcustomerissues;
        return CustomerIssuesServiceFactory;
    }
})();