//'use strict';
//app.factory('invoivceService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var invoiceServiceFactory = {};

//    //var _gettasks = function () {

//    //    return $http.get(serviceBase + 'api/ProjectTask').then(function (results) {
//    //        return results;
//    //    });
//    //};

//    //tasksServiceFactory.gettasks = _gettasks;



//    //var _puttasks = function () {

//    //    return $http.put(serviceBase + 'api/ProjectTask').then(function (results) {
//    //        return results;
//    //    });
//    //};

//    //tasksServiceFactory.puttasks = _puttasks;



//    var _putinvoice = function () {

//        return $http.post(serviceBase + 'api/AllInvoice').then(function (results) {
//            return results;
//        });
//    };

//    invoiceServiceFactory.putinvoice = _putinvoice;




//    //var _deletetasks = function (data) {
//    //    console.log("Delete Calling");
//    //    console.log(data.Id);


//    //    return $http.delete(serviceBase + 'api/ProjectTask/?id=' + data.Id).then(function (results) {
//    //        return results;
//    //    });
//    //};

//    //tasksServiceFactory.deletetasks = _deletetasks;
//    //tasksServiceFactory.gettasks = _gettasks;





//    return invoiceServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('invoivceService', invoivceService);

    invoivceService.$inject = ['$http', 'ngAuthSettings'];

    function invoivceService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var invoiceServiceFactory = {};

        //var _gettasks = function () {

        //    return $http.get(serviceBase + 'api/ProjectTask').then(function (results) {
        //        return results;
        //    });
        //};

        //tasksServiceFactory.gettasks = _gettasks;



        //var _puttasks = function () {

        //    return $http.put(serviceBase + 'api/ProjectTask').then(function (results) {
        //        return results;
        //    });
        //};

        //tasksServiceFactory.puttasks = _puttasks;



        var _putinvoice = function () {

            return $http.post(serviceBase + 'api/AllInvoice').then(function (results) {
                return results;
            });
        };

        invoiceServiceFactory.putinvoice = _putinvoice;




        //var _deletetasks = function (data) {
        //    console.log("Delete Calling");
        //    console.log(data.Id);


        //    return $http.delete(serviceBase + 'api/ProjectTask/?id=' + data.Id).then(function (results) {
        //        return results;
        //    });
        //};

        //tasksServiceFactory.deletetasks = _deletetasks;
        //tasksServiceFactory.gettasks = _gettasks;





        return invoiceServiceFactory;
    }
})();