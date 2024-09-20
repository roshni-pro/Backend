//'use strict';
//app.factory('authInterceptorService', ['$q', '$injector', '$location', 'localStorageService', '$rootScope', function ($q, $injector, $location, localStorageService, $rootScope) {

//    var authInterceptorServiceFactory = {};

//    var _request = function (config) {

//        config.headers = config.headers || {};

//        var authData = localStorageService.get('authorizationData');
//        if (authData) {
//            config.headers.Authorization = 'Bearer ' + authData.token;
//        }

//        return config;
//    }

//    var _responseError = function (rejection) {
//        if (rejection.status === 401) {
//            var authService = $injector.get('authService');
//            var authData = localStorageService.get('authorizationData');

//            if (authData) {
//                if (authData.useRefreshTokens) {
//                    $location.path('/refresh');
//                    return $q.reject(rejection);
//                }
//            }
//            authService.logOut();

//            $location.path('/pages/signin');
//        }
//        return $q.reject(rejection);
//    }


//    var _response = function (response) {
        
//        if (response.config.url.indexOf("api/") > -1) {
//            // var d = JSON.parse(AES256.decrypt(response.data.Data, 'Sh0pK!r@n@#@!@#$'));
//            if (response.data.Status == 'OK') {

//                var today = new Date();
//                var month = pad(today.getMonth() + 1, 2);
//                var date = pad(today.getDate(), 2);
//                response.data = JSON.parse(AES256.decrypt(response.data.Data, today.getFullYear() + "" + month + "" + date + "1201"));

//            }
//            else {
//                var deferred = $q.defer();
//                return $q.reject(response.data.ErrorMessage);
//            }
//        }
        
//        return response;
//    }

//    function pad(n, width, z) {
//        z = z || '0';
//        n = n + '';
//        return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
//    }

//    authInterceptorServiceFactory.request = _request;
//    authInterceptorServiceFactory.response = _response;
//    authInterceptorServiceFactory.responseError = _responseError;

//    return authInterceptorServiceFactory;
//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('authInterceptorService', authInterceptorService);

    authInterceptorService.$inject = ['$q', '$injector', '$location', 'localStorageService', '$rootScope'];

    function authInterceptorService($q, $injector, $location, localStorageService, $rootScope) {

        var authInterceptorServiceFactory = {};

        var _request = function (config) {
            $rootScope.showHide = true;
            config.headers = config.headers || {};

            var authData = localStorageService.get('authorizationData');
            if (authData) {
                config.headers.Authorization = 'Bearer ' + authData.token;
            }

            return config;
        }

        var _responseError = function (rejection) {
            $rootScope.showHide = false;
            if (rejection.status === 401) {
                var authService = $injector.get('authService');
                var authData = localStorageService.get('authorizationData');

                if (authData) {
                    if (authData.useRefreshTokens) {
                        $location.path('/refresh');
                        return $q.reject(rejection);
                    }
                }
                authService.logOut();

                $location.path('/pages/signin');
            }
            return $q.reject(rejection);
        }


        var _response = function (response) {
            $rootScope.showHide = false;
            if (response.config.url.indexOf("api/") > -1) {
                // var d = JSON.parse(AES256.decrypt(response.data.Data, 'Sh0pK!r@n@#@!@#$'));
           
                if (response.data.Status == 'OK') {
                    var today = new Date();
                    var month = pad(today.getMonth() + 1, 2);
                    var date = pad(today.getDate(), 2);
                    response.data = response.data.Data ?
                        JSON.parse(AES256.decrypt(response.data.Data, today.getFullYear() + "" + month + "" + date + "1201"))
                        :"";

                }               
                else {
                    var deferred = $q.defer();
                    return $q.reject(response.data.ErrorMessage);
                }
            }

            return response;
        }

        function pad(n, width, z) {
            z = z || '0';
            n = n + '';
            return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
        }

        authInterceptorServiceFactory.request = _request;
        authInterceptorServiceFactory.response = _response;
        authInterceptorServiceFactory.responseError = _responseError;

        return authInterceptorServiceFactory;
    }
})()