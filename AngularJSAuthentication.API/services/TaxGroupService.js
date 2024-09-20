//'use strict';
//app.factory('TaxGroupService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in taxgroup service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var TaxGroupServiceFactory = {};

//    var _getTaxGroup = function () {

//        return $http.get(serviceBase + 'api/TaxGroup').then(function (results) {
//            return results;
//        });
//    };

//    TaxGroupServiceFactory.getTaxGroup = _getTaxGroup;



//    var _putTaxGroup = function () {

//        return $http.put(serviceBase + 'api/TaxGroup').then(function (results) {
//            console.log("putstates");
//            console.log(results);
//            return results;
//        });
//    };

//    TaxGroupServiceFactory.putTaxGroup = _putTaxGroup;




//    var _deleteTaxGroup = function (data) {
//        console.log("Delete Calling");
//        console.log(data.GruopID);


//        return $http.delete(serviceBase + 'api/TaxGroup/?id=' + data.GruopID).then(function (results) {
//            return results;
//        });
//    };

//    TaxGroupServiceFactory.deleteTaxGroup = _deleteTaxGroup;
//    TaxGroupServiceFactory.getTaxGroup = _getTaxGroup;





//    return TaxGroupServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('TaxGroupService', TaxGroupService);

    TaxGroupService.$inject = ['$http', 'ngAuthSettings'];

    function TaxGroupService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var TaxGroupServiceFactory = {};

        var _getTaxGroup = function () {

            return $http.get(serviceBase + 'api/TaxGroup').then(function (results) {
                return results;
            });
        };

        TaxGroupServiceFactory.getTaxGroup = _getTaxGroup;



        var _putTaxGroup = function () {

            return $http.put(serviceBase + 'api/TaxGroup').then(function (results) {
                console.log("putstates");
                console.log(results);
                return results;
            });
        };

        TaxGroupServiceFactory.putTaxGroup = _putTaxGroup;




        var _deleteTaxGroup = function (data) {
            console.log("Delete Calling");
            console.log(data.GruopID);


            return $http.delete(serviceBase + 'api/TaxGroup/?id=' + data.GruopID).then(function (results) {
                return results;
            });
        };

        TaxGroupServiceFactory.deleteTaxGroup = _deleteTaxGroup;
        TaxGroupServiceFactory.getTaxGroup = _getTaxGroup;





        return TaxGroupServiceFactory;
    }
})();