
(function () {
    'use strict';

    angular
        .module('app')
        .factory('SubsubCategoryService', SubsubCategoryService);

    SubsubCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function SubsubCategoryService($http, ngAuthSettings) {

        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var SubsubCategoryServiceFactory = {};

        var _getsubsubcats = function () {

            return $http.get(serviceBase + 'api/SubsubCategory/Allsubsubcategory').then(function (results) {
                return results;
            });
        };

        SubsubCategoryServiceFactory.getsubsubcats = _getsubsubcats;


        // for whsubsub
        var _getWarhouseCategory = function (data) {

            console.log("Service");
            console.log("get Filter Warhouse category function in warehouse service");
            console.log("ID");
            console.log(data.WarehouseId);

            return $http.get(serviceBase + 'api/Category', {
                params: {
                    recordtype: "warehouse",
                    whid: data.WarehouseId
                }
            }).success(function (data, status) {
                console.log(data);
                return data;
            });
        };

        SubsubCategoryServiceFactory.getWarhouseCategory = _getWarhouseCategory;
        ////
        var _getWarhouseCategorybyid = function (data) {

            console.log("Service");
            console.log("get Filter Warhouse categor by id function in warehouse service");
            console.log("ID");
            console.log(data.Warehouseid);
            console.log("WHID");
            console.log(data.WhCategoryid);

            return $http.get(serviceBase + 'api/Category', {
                params: {
                    recordtype: "warehouse",
                    whid: data.WarehouseId,
                    whcatid: data.WhCategoryid
                }
            }).success(function (data, status) {
                console.log(data);
                return data;
            });
        };

        SubsubCategoryServiceFactory.getWarhouseCategorybyid = _getWarhouseCategorybyid;
        /////










        var _putsubsubcats = function () {

            return $http.put(serviceBase + 'api/SubsubCategory').then(function (results) {
                return results;
            });
        };

        SubsubCategoryServiceFactory.putsubsubcats = _putsubsubcats;




        var _deletesubsubcategorys = function (data) {
            console.log("Delete Calling");
            console.log(data.SubsubCategoryid);


            return $http.delete(serviceBase + 'api/SubsubCategory/?id=' + data.SubsubCategoryid).then(function (results) {
                return results;
            });
        };

        SubsubCategoryServiceFactory.deletesubsubcategorys = _deletesubsubcategorys;
        SubsubCategoryServiceFactory.getsubsubcats = _getsubsubcats;





        return SubsubCategoryServiceFactory;
    }
})();