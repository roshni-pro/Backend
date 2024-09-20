//'use strict';
//app.factory('FinancialYearService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in FinancialYear ");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var FinancialYearServiceFactory = {};

//    var _getfinancialYear = function () {

//        return $http.get(serviceBase + 'api/FinancialYear').then(function (results) {
//            return results;
//        });
//    };

//    FinancialYearServiceFactory.getfinancialYear = _getfinancialYear;



//    var _putfinancialYear = function () {

//        return $http.put(serviceBase + 'api/FinancialYear').then(function (results) {
//            console.log("_financialYear");
//            console.log(results);
//            return results;
//        });
//    };

//    FinancialYearServiceFactory.putfinancialYear = _putfinancialYear;




//    var _deletefinancialYear = function (data) {
//        console.log("Delete Calling");
       


//        return $http.delete(serviceBase + 'api/FinancialYear/?id=' + data.Financialyearid).then(function (results) {
//            return results;
//        });
//    };

//    FinancialYearServiceFactory.deletefinancialYear = _deletefinancialYear;
//    FinancialYearServiceFactory.getfinancialYear = _getfinancialYear;





//    return FinancialYearServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('FinancialYearService', FinancialYearService);

    FinancialYearService.$inject = ['$http', 'ngAuthSettings'];

    function FinancialYearService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var FinancialYearServiceFactory = {};

        var _getfinancialYear = function () {

            return $http.get(serviceBase + 'api/FinancialYear').then(function (results) {
                return results;
            });
        };

        FinancialYearServiceFactory.getfinancialYear = _getfinancialYear;



        var _putfinancialYear = function () {

            return $http.put(serviceBase + 'api/FinancialYear').then(function (results) {
                console.log("_financialYear");
                console.log(results);
                return results;
            });
        };

        FinancialYearServiceFactory.putfinancialYear = _putfinancialYear;




        var _deletefinancialYear = function (data) {
            console.log("Delete Calling");



            return $http.delete(serviceBase + 'api/FinancialYear/?id=' + data.Financialyearid).then(function (results) {
                return results;
            });
        };

        FinancialYearServiceFactory.deletefinancialYear = _deletefinancialYear;
        FinancialYearServiceFactory.getfinancialYear = _getfinancialYear;





        return FinancialYearServiceFactory;
    }
})();