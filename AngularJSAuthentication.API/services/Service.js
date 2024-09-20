//'use strict';
//app.factory('Service', ['$http', 'ngAuthSettings', function ('$http', 'ngAuthSettings') {
//    var ServiceFactory = {};
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;
//    var _get = function (controller) {
//        $(".overlay").show();
//        var url = serviceBase+"api/"+controller;
//        return $http.get(url).then(function (results) {
//            $(".overlay").hide();
//            return results;
//        }, function (error) {
//            $(".overlay").hide();
//        });
//    };
//    ServiceFactory.get = _get;

//    var _getbyId = function (controller,id) {
//        $(".overlay").show();
//        var url = serviceBase + "api/" + controller + "?id=" + id;
//        return $http.get(url,id).then(function (results) {
//            $(".overlay").hide();
//            return results;
//        }, function (error) {
//            $(".overlay").hide();
//        });
//    };
//    ServiceFactory.getbyId = _getbyId;

//    var _signin = function (controller, data) {
//         $(".overlay").show();
//        var url = serviceBase + "api/" + controller + "?Email=" + data.Email + "&Password=" + data.Password;
//        return $http.get(url, data).then(function (results) {
//            $(".overlay").hide();  return results;
//        }, function (error) {
//            $(".overlay").hide();
//        });
//    };
//    ServiceFactory.signin = _signin;

//    var _post = function (controller,data) {
//        $(".overlay").show();
        
//        var url = serviceBase + "api/" + controller;
//        return $http.post(url,data).then(function (results) {
//            $(".overlay").hide();
//            return results;
//        },function(error){
//            $(".overlay").hide();
//        });
//    };
//    ServiceFactory.post = _post;


//    var _put = function (controller, value) {
//        var url = serviceBase + "api/" + controller;
//        $(".overlay").show();
//        return $http.put(url,value).then(function (results) {
//            $(".overlay").hide();
//            return results;
//        }, function (error) {
//            $(".overlay").hide();
//        });
//    };
//    ServiceFactory.put = _put;

//    var _delete = function (controller, id) {
//        $(".overlay").show();
//        var url = serviceBase + "api/" + controller;
//        return $http.delete(url,id).then(function (results) {
//            $(".overlay").hide();
//            return results;
//        }, function (error) {
//            $(".overlay").hide();
//        });
//    };
//    ServiceFactory.delete = _delete;
    
//    return ServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('Service', Service);

    Service.$inject = ['$http', 'ngAuthSettings'];

    function Service($http, ngAuthSettings) {
        var ServiceFactory = {};
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var _get = function (controller) {
            $(".overlay").show();
            var url = serviceBase + "api/" + controller;
            return $http.get(url).then(function (results) {
                $(".overlay").hide();
                return results;
            }, function (error) {
                $(".overlay").hide();
            });
        };
        ServiceFactory.get = _get;

        var _getbyId = function (controller, id) {
            $(".overlay").show();
            var url = serviceBase + "api/" + controller + "?id=" + id;
            return $http.get(url, id).then(function (results) {
                $(".overlay").hide();
                return results;
            }, function (error) {
                $(".overlay").hide();
            });
        };
        ServiceFactory.getbyId = _getbyId;

        var _signin = function (controller, data) {
            $(".overlay").show();
            var url = serviceBase + "api/" + controller + "?Email=" + data.Email + "&Password=" + data.Password;
            return $http.get(url, data).then(function (results) {
                $(".overlay").hide(); return results;
            }, function (error) {
                $(".overlay").hide();
            });
        };
        ServiceFactory.signin = _signin;

        var _post = function (controller, data) {
            $(".overlay").show();

            var url = serviceBase + "api/" + controller;
            return $http.post(url, data).then(function (results) {
                $(".overlay").hide();
                return results;
            }, function (error) {
                $(".overlay").hide();
            });
        };
        ServiceFactory.post = _post;


        var _put = function (controller, value) {
            var url = serviceBase + "api/" + controller;
            $(".overlay").show();
            return $http.put(url, value).then(function (results) {
                $(".overlay").hide();
                return results;
            }, function (error) {
                $(".overlay").hide();
            });
        };
        ServiceFactory.put = _put;

        var _delete = function (controller, id) {
            $(".overlay").show();
            var url = serviceBase + "api/" + controller;
            return $http.delete(url, id).then(function (results) {
                $(".overlay").hide();
                return results;
            }, function (error) {
                $(".overlay").hide();
            });
        };
        ServiceFactory.delete = _delete;

        return ServiceFactory;
    }
})();