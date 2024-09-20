//'use strict';
//app.factory('TaxMasterService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in taxmaster service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var TaxMasterServiceServiceFactory = {};

//    var _getTaxmaster = function () {

//        return $http.get(serviceBase + 'api/TaxMaster').then(function (results) {
//            return results;
//        });
//    };

//    TaxMasterServiceServiceFactory.getTaxmaster = _getTaxmaster;



//    var _putTaxMaster = function () {

//        return $http.put(serviceBase + 'api/TaxMaster').then(function (results) {
//            console.log("putstates");
//            console.log(results);
//            return results;
//        });
//    };

//    TaxMasterServiceServiceFactory.putTaxMaster = _putTaxMaster;




//    var _deletesTaxmaster = function (data) {
//        console.log("Delete Calling");
//        console.log(data.TaxID);


//        return $http.delete(serviceBase + 'api/TaxMaster/?id=' + data.TaxID).then(function (results) {
//            return results;
//        });
//    };

//    TaxMasterServiceServiceFactory.deletesTaxmaster = _deletesTaxmaster;
//    TaxMasterServiceServiceFactory.getTaxmaster = _getTaxmaster;





//    return TaxMasterServiceServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('TaxMasterService', TaxMasterService);

    TaxMasterService.$inject = ['$http', 'ngAuthSettings'];

    function TaxMasterService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var TaxMasterServiceServiceFactory = {};

        var _getTaxmaster = function () {

            return $http.get(serviceBase + 'api/TaxMaster').then(function (results) {
                return results;
            });
        };

        TaxMasterServiceServiceFactory.getTaxmaster = _getTaxmaster;



        var _putTaxMaster = function () {

            return $http.put(serviceBase + 'api/TaxMaster').then(function (results) {
                console.log("putstates");
                console.log(results);
                return results;
            });
        };

        TaxMasterServiceServiceFactory.putTaxMaster = _putTaxMaster;




        var _deletesTaxmaster = function (data) {
            console.log("Delete Calling");
            console.log(data.TaxID);


            return $http.delete(serviceBase + 'api/TaxMaster/?id=' + data.TaxID).then(function (results) {
                return results;
            });
        };

        TaxMasterServiceServiceFactory.deletesTaxmaster = _deletesTaxmaster;
        TaxMasterServiceServiceFactory.getTaxmaster = _getTaxmaster;





        return TaxMasterServiceServiceFactory;
    }
})();