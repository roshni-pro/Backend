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

//app.factory('ItemPramotionService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var pramotionServiceFactory = {};
//    var _getitems = function (id) {

//        return $http.get(serviceBase + 'api/ItemJson?id=' + id).then(function (results) {
//            return results;
//        })
//    };
//    var _getitempramotion = function () {
//        console.log("get item pramotion service is calling")
//        return $http.get(serviceBase + 'api/pramotion').then(function (results) {
//            return results;
//        })
//    };
//    var _Postitempramotion = function (data) {
//        console.log("Post Postitempramotion service is calling....");
//        console.log(data);

//        var url = serviceBase + 'api/pramotion';
//        return $http.post(url, data).then(function (results) {
//            return results;
//        });
//    }

//    pramotionServiceFactory.getitempramotion = _getitempramotion;
//    pramotionServiceFactory.Postitempramotion = _Postitempramotion;
//    pramotionServiceFactory.getitems = _getitems;

//    return pramotionServiceFactory;
//}])

    (function () {
        'use strict';

        angular
            .module('app')
            .factory('ItemPramotionService', ItemPramotionService);

        ItemPramotionService.$inject = ['$http', 'ngAuthSettings'];

        function ItemPramotionService($http, ngAuthSettings) {
            var serviceBase = ngAuthSettings.apiServiceBaseUri;

            var pramotionServiceFactory = {};
            var _getitems = function (id) {

                return $http.get(serviceBase + 'api/ItemJson?id=' + id).then(function (results) {
                    return results;
                })
            };
            var _getitempramotion = function () {
                console.log("get item pramotion service is calling")
                return $http.get(serviceBase + 'api/pramotion').then(function (results) {
                    return results;
                })
            };
            var _Postitempramotion = function (data) {
                console.log("Post Postitempramotion service is calling....");
                console.log(data);

                var url = serviceBase + 'api/pramotion';
                return $http.post(url, data).then(function (results) {
                    return results;
                });
            }

            pramotionServiceFactory.getitempramotion = _getitempramotion;
            pramotionServiceFactory.Postitempramotion = _Postitempramotion;
            pramotionServiceFactory.getitems = _getitems;

            return pramotionServiceFactory;
        }
    })();