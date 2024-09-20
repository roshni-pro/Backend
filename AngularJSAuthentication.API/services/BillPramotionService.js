//'use strict';
app.filter('unique', function () {

    return function (items, filterOn) {

        if (filterOn === false) {
            return items;
        }

        if ((filterOn || angular.isUndefined(filterOn)) && angular.isArray(items)) {
            var hashCheck = {}, newItems = [];

            var extractValueToCompare = function (item) {
                if (angular.isObject(item) && angular.isString(filterOn)) {
                    return item[filterOn];
                } else {
                    return item;
                }
            };

            angular.forEach(items, function (item) {
                var valueToCheck, isDuplicate = false;

                for (var i = 0; i < newItems.length; i++) {
                    if (angular.equals(extractValueToCompare(newItems[i]), extractValueToCompare(item))) {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate) {
                    newItems.push(item);
                }

            });
            items = newItems;
        }
        return items;
    };
});

//app.factory('BillPramotionService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var billpramotionServiceFactory = {};
  
//    var _getbillpramotion = function () {
//        console.log("get item pramotion service is calling")
//        return $http.get(serviceBase + 'api/billpramotion').then(function (results) {
//            return results;
//        })
//    };
//    var _Postbillpramotion = function (data) {
//        console.log("Post Postitempramotion service is calling....");
//        console.log(data);

//        var url = serviceBase + 'api/billpramotion';
//        return $http.post(url, data).then(function (results) {
//            return results;
//        });
//    }

//    billpramotionServiceFactory.getbillpramotion = _getbillpramotion;
//    billpramotionServiceFactory.Postbillpramotion = _Postbillpramotion;
  
//    return billpramotionServiceFactory;
//}])


    (function () {
        'use strict';

        angular
            .module('app')
            .factory('BillPramotionService', BillPramotionService);

        BillPramotionService.$inject = ['$http', 'ngAuthSettings'];

        function BillPramotionService($http, ngAuthSettings) {
            var serviceBase = ngAuthSettings.apiServiceBaseUri;

            var billpramotionServiceFactory = {};

            var _getbillpramotion = function () {
                console.log("get item pramotion service is calling")
                return $http.get(serviceBase + 'api/billpramotion').then(function (results) {
                    return results;
                })
            };
            var _Postbillpramotion = function (data) {
                console.log("Post Postitempramotion service is calling....");
                console.log(data);

                var url = serviceBase + 'api/billpramotion';
                return $http.post(url, data).then(function (results) {
                    return results;
                });
            }

            billpramotionServiceFactory.getbillpramotion = _getbillpramotion;
            billpramotionServiceFactory.Postbillpramotion = _Postbillpramotion;

            return billpramotionServiceFactory;
        }
    })();