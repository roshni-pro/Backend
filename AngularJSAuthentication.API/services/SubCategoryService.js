//'use strict';
//app.factory('SubCategoryService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("insubcat service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var SubCategoryServiceFactory = {};

//    var _getsubcategorys = function () {

//        return $http.get(serviceBase + 'api/SubCategory').then(function (results) {
//            return results;
//        });
//    };    

//    SubCategoryServiceFactory.getsubcategorys = _getsubcategorys;
     
//    //============================================================================================================
//    var _getWarhouseSubCategory = function (data) {
//        console.log("Service");
//        console.log("get Filter Warhouse sub category function in warehouse service");
//        console.log("ID");
//        console.log(data.Warehouseid);

//        return $http.get(serviceBase + 'api/SubCategory', {
//            params: {
//                recordtype: "warehouse",
//                whid: data.Warehouseid
//            }
//        }).success(function (data, status) {
//            console.log(data);
//            return data;
//        });
//    };

//    SubCategoryServiceFactory.getWarhouseSubCategory = _getWarhouseSubCategory;

//    //===================================================================================================
//    var _putsubcategorys = function () {

//        return $http.put(serviceBase + 'api/SubCategory').then(function (results) {
//            return results;
//        });
//    };

//    SubCategoryServiceFactory.putsubcategorys = _putsubcategorys;




//    var _deletesubcategorys = function (data) {
//        console.log("Delete Calling");
//        console.log(data.SubCategoryId);


//        return $http.delete(serviceBase + 'api/SubCategory/?id=' + data.SubCategoryId).then(function (results) {
//            return results;
//        });
//    };

//    SubCategoryServiceFactory.deletesubcategorys = _deletesubcategorys;
//    SubCategoryServiceFactory.getsubcategorys = _getsubcategorys;





//    return SubCategoryServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('SubCategoryService', SubCategoryService);

    SubCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function SubCategoryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var SubCategoryServiceFactory = {};

        var _getsubcategorys = function () {

            return $http.get(serviceBase + 'api/SubCategory/getallsubcategory').then(function (results) {
                return results;
            });
        };

        SubCategoryServiceFactory.getsubcategorys = _getsubcategorys;

        //============================================================================================================
        var _getWarhouseSubCategory = function (data) {
            console.log("Service");
            console.log("get Filter Warhouse sub category function in warehouse service");
            console.log("ID");
            console.log(data.Warehouseid);

            return $http.get(serviceBase + 'api/SubCategory', {
                params: {
                    recordtype: "warehouse",
                    whid: data.Warehouseid
                }
            }).success(function (data, status) {
                console.log(data);
                return data;
            });
        };

        SubCategoryServiceFactory.getWarhouseSubCategory = _getWarhouseSubCategory;

        //===================================================================================================
        var _putsubcategorys = function () {

            return $http.put(serviceBase + 'api/SubCategory').then(function (results) {
                return results;
            });
        };

        SubCategoryServiceFactory.putsubcategorys = _putsubcategorys;




        var _deletesubcategorys = function (data) {
            console.log("Delete Calling");
            console.log(data.SubCategoryId);


            return $http.delete(serviceBase + 'api/SubCategory/?id=' + data.SubCategoryId).then(function (results) {
                return results;
            });
        };

        SubCategoryServiceFactory.deletesubcategorys = _deletesubcategorys;
        SubCategoryServiceFactory.getsubcategorys = _getsubcategorys;





        return SubCategoryServiceFactory;
    }
})();