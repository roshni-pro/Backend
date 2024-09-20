'use strict';
app.factory('CurrencyStockService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
    console.log("in currency Stock ");
    var serviceBase = ngAuthSettings.apiServiceBaseUri;
    var CurrecyStockServiceFactory = {};
    
    var _getCurrecyStock = function (Status) {
        return $http.get(serviceBase + 'api/CurrencyStock').then(function (results) {
             //return $http.get(serviceBase + 'api/CurrencyStock?Stock_status=' + Status).then(function (results) {
            return results;
        });
    };
    CurrecyStockServiceFactory.getCurrecyStock = _getCurrecyStock;

    ////getdatabyid
    //var _getCurrecyStockbyId = function (Status) {
    //    return $http.get(serviceBase + 'api/CurrencyStock?Status=' + Status).then(function (results) {
    //        return results;
    //    })
    //}



}]);

