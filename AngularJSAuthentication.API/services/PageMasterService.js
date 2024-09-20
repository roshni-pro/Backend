//'use strict';
//app.factory('PageMasterService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var PageMasterServiceFactory = {};

//    var _getPageMaster = function () {      
//        console.log("in PageMaster service")

//        return $http.get(serviceBase + 'api/PageMaster/GetAllParentPagesForDropDown').then(function (results) {

//            return results;
//        });
//    };
//    PageMasterServiceFactory.getPageMaster = _getPageMaster;

//    return PageMasterServiceFactory;
//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('PageMasterService', PageMasterService);

    PageMasterService.$inject = ['$http', 'ngAuthSettings'];

    function PageMasterService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var PageMasterServiceFactory = {};

        var _getPageMaster = function () {
            console.log("in PageMaster service")

            return $http.get(serviceBase + 'api/PageMaster/GetAllParentPagesForDropDown').then(function (results) {

                return results;
            });
        };
        PageMasterServiceFactory.getPageMaster = _getPageMaster;

        return PageMasterServiceFactory;
    }
})();