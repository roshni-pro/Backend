//'use strict';
//app.factory('authService', ['$http', '$q', 'localStorageService', 'ngAuthSettings', function ($http, $q, localStorageService, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;
//    var authServiceFactory = {};

//    var _authentication = {
//        isAuth: false,
//        userName: "",
//        CompanyId: 0,
//        role: "",
//        userid: 0,
//        Warehouseid:0,
//        useRefreshTokens: false
//    };

//    var _externalAuthData = {
//        provider: "",
//        userName: "",
//        externalAccessToken: ""
//    };

//    var _isAdmin = function () {
        
//        if (_authentication.isAuth) {
//            if (_authentication.role.trim().toLowerCase() == "HQ Master login") {
//                console.log("is admin in authservice");
//                return true;
//            }
//            if (_authentication.role.trim().toLowerCase() == "Operations HQ Master login") {
//                console.log("is Operations HQ Master login in authservice");
//                return true;
//            }
//            if (_authentication.role.trim().toLowerCase() == "Purchase HQ Master login") {
//                console.log("is Purchase HQ Master login in authservice");
//                return true;
//            }
//            if (_authentication.role.trim().toLowerCase() == "Statistics HQ Master login") {
//                console.log("is Purchase HQ Master login in authservice");
//                return true;
//            }
            
//            if (_authentication.role.trim().toLowerCase() == "Sales Executive") {
//                console.log("is Sales Executive in authservice");
//                return true;
//            }
//            if (_authentication.role.trim().toLowerCase() == "Super Sales") {
//                console.log("Super Sales");
//                return true;
//            }
//            if (_authentication.role.trim().toLowerCase() == "WH Master login") {
//                console.log("WH Master login");
//                return true;
               
//            }
//            if (_authentication.role.trim().toLowerCase() == "SaleSettle HQ Master login") {
//                console.log("SaleSettle HQ Master login");
//                return true;

//            }

//            if (_authentication.role.trim().toLowerCase() == "others") {
//                console.log("isothersin authservice");
//                return true;
//            }
//            return false;
//        }
//    }

//    var _saveRegistration = function (registration) {
//        return $http.post(serviceBase + 'api/account/register', registration).success(function (response) {
//            console.log(response);
//            response.status = "ok";
//            return response;
//        }).error(function (data, status, headers, config) {
//            console.log("saved comment", data);
//            return data;
//        });
//    };

//    var _saveRegistrationpeople = function (registration) {
        
//        return $http.post(serviceBase + 'api/account/register', registration).success(function (response) {
//            console.log(response);
//            return response;
//        }).error(function (data, status, headers, config) {
//            console.log("saved comment", data);
//            return data;
//        });
//    };

//    var _StudRegistration=function(registration)
//    {
//        console.log("Now this method is calling"); 
//        return $http.post(serviceBase + 'api/account/StudentRegister', registration).then(function (response) {
//            return response;
//        });
//    }

//    var _login = function (loginData) {
        
//        //_logOut();
//        var data = "grant_type=password&username="+ loginData.userName + "&password="+ loginData.password;

//        if (loginData.useRefreshTokens) {
//            data = data + "&client_id="+ ngAuthSettings.clientId;
//        }

//        var deferred = $q.defer();
//        console.log("Calling login post");
//        console.log(data);
        
//        $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
            
//            console.log(response);
            
//            if (loginData.useRefreshTokens) {
//                localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, CompanyId: response.compid, role: response.role, refreshToken: response.refresh_token, userid: response.userid, Warehouseid: response.Warehouseid, useRefreshTokens: true, PagePermission: response.pagePermissions });
//            }
//            else
//            {
                
//                localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, CompanyId: response.compid, role: response.role, refreshToken: "", userid: response.userid, Warehouseid: response.Warehouseid, useRefreshTokens: false, PagePermission: response.pagePermissions });
//            }
//            _authentication.isAuth = true;
//            _authentication.userName = loginData.userName;
//            _authentication.role = response.role;
//            _authentication.userid = response.userid;
//            _authentication.Warehouseid = response.Warehouseid;
//            _authentication.compid = response.compid;
//            _authentication.useRefreshTokens = loginData.useRefreshTokens;
//            console.log("Authentication Token")
//            console.log(_authentication);
//            deferred.resolve(response);
//        }).error(function (err, status) {
//            console.log("Failed, " + err +", "+status);
//            _logOut();
//            deferred.reject(err);
//        });
//        return deferred.promise;
//    };

//    var _logOut = function () {
        
//        localStorageService.remove('authorizationData');
//        _authentication.isAuth = false;
//        _authentication.userName = "";
//        _authentication.useRefreshTokens = false;
//    };

//    var _fillAuthData = function () {
        
//        var authData = localStorageService.get('authorizationData');
//        if (authData) {
//            _authentication.isAuth = true;
//            _authentication.userName = authData.userName;
//            _authentication.useRefreshTokens = authData.useRefreshTokens;
//        }
//    };

//    var _refreshToken = function () {
        
//        var deferred = $q.defer();
//        var authData = localStorageService.get('authorizationData');
//        if (authData) {
//            if (authData.useRefreshTokens) {
//                var data = "grant_type=refresh_token&refresh_token="+ authData.refreshToken + "&client_id="+ ngAuthSettings.clientId;
//                localStorageService.remove('authorizationData');
//                $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
//                    localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: response.refresh_token, useRefreshTokens: true });
//                    deferred.resolve(response);
//                }).error(function (err, status) {
//                    _logOut();
//                    deferred.reject(err);
//                });
//            }
//        }
//        return deferred.promise;
//    };

//    var _obtainAccessToken = function (externalData) {
//        var deferred = $q.defer();
        
//        $http.get(serviceBase + 'api/account/ObtainLocalAccessToken', { params: { provider: externalData.provider, externalAccessToken: externalData.externalAccessToken } }).success(function (response) {

//            localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: "", useRefreshTokens: false });

//            _authentication.isAuth = true;
//            _authentication.userName = response.userName;
//            _authentication.useRefreshTokens = false;
//            deferred.resolve(response);

//        }).error(function (err, status) {
//            _logOut();
//            deferred.reject(err);
//        });
//        return deferred.promise;
//    };

//    var _registerExternal = function (registerExternalData) {

//        var deferred = $q.defer();
//        $http.post(serviceBase + 'api/account/registerexternal', registerExternalData).success(function (response) {
//            localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: "", useRefreshTokens: false });

//            _authentication.isAuth = true;
//            _authentication.userName = response.userName;
//            _authentication.useRefreshTokens = false;
//            deferred.resolve(response);
//        }).error(function (err, status) {
//            _logOut();
//            deferred.reject(err);
//        });
//        return deferred.promise;
//    };
    
//    authServiceFactory.saveRegistration = _saveRegistration;
//    authServiceFactory.saveRegistrationpeople = _saveRegistrationpeople;
//    authServiceFactory.login = _login;
//    authServiceFactory.logOut = _logOut;
//    authServiceFactory.fillAuthData = _fillAuthData;
//    authServiceFactory.authentication = _authentication;
//    authServiceFactory.refreshToken = _refreshToken;
//    authServiceFactory.isAdmin = _isAdmin;
//    authServiceFactory.obtainAccessToken = _obtainAccessToken;
//    authServiceFactory.externalAuthData = _externalAuthData;
//    authServiceFactory.registerExternal = _registerExternal;

//    return authServiceFactory;
//}]);


(function () {
    'use strict';

    angular
        .module('app')
        .factory('authService', authService);

    authService.$inject = ['$http', '$q', 'localStorageService', 'ngAuthSettings'];

    function authService($http, $q, localStorageService, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var authServiceFactory = {};

        var _authentication = {
            isAuth: false,
            userName: "",
            CompanyId: 0,
            role: "",
            userid: 0,
            Warehouseid: 0,
            useRefreshTokens: false
        };

        var _externalAuthData = {
            provider: "",
            userName: "",
            externalAccessToken: ""
        };

        var _isAdmin = function () {

            if (_authentication.isAuth) {
                if (_authentication.role.trim().toLowerCase() == "HQ Master login") {
                    console.log("is admin in authservice");
                    return true;
                }
                if (_authentication.role.trim().toLowerCase() == "Operations HQ Master login") {
                    console.log("is Operations HQ Master login in authservice");
                    return true;
                }
                if (_authentication.role.trim().toLowerCase() == "Purchase HQ Master login") {
                    console.log("is Purchase HQ Master login in authservice");
                    return true;
                }
                if (_authentication.role.trim().toLowerCase() == "Statistics HQ Master login") {
                    console.log("is Purchase HQ Master login in authservice");
                    return true;
                }

                if (_authentication.role.trim().toLowerCase() == "Sales Executive") {
                    console.log("is Sales Executive in authservice");
                    return true;
                }
                if (_authentication.role.trim().toLowerCase() == "Super Sales") {
                    console.log("Super Sales");
                    return true;
                }
                if (_authentication.role.trim().toLowerCase() == "WH Master login") {
                    console.log("WH Master login");
                    return true;

                }
                if (_authentication.role.trim().toLowerCase() == "SaleSettle HQ Master login") {
                    console.log("SaleSettle HQ Master login");
                    return true;

                }

                if (_authentication.role.trim().toLowerCase() == "others") {
                    console.log("isothersin authservice");
                    return true;
                }
                return false;
            }
        }

        var _saveRegistration = function (registration) {
            return $http.post(serviceBase + 'api/account/register', registration).success(function (response) {
                console.log(response);
                response.status = "ok";
                return response;
            }).error(function (data, status, headers, config) {
                console.log("saved comment", data);
                return data;
            });
        };

        var _saveRegistrationpeople = function (registration) {

            return $http.post(serviceBase + 'api/account/register', registration).success(function (response) {
                console.log(response);
                return response;
            }).error(function (data, status, headers, config) {
                console.log("saved comment", data);
                return data;
            });
        };

        var _StudRegistration = function (registration) {
            console.log("Now this method is calling");
            return $http.post(serviceBase + 'api/account/StudentRegister', registration).then(function (response) {
                return response;
            });
        }

        var _login = function (loginData) {

            //_logOut();
            var data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password;

            if (loginData.useRefreshTokens) {
                data = data + "&client_id=" + ngAuthSettings.clientId;
            }

            var deferred = $q.defer();
            console.log("Calling login post");
            console.log(data);

            $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                console.log(response);

                if (loginData.useRefreshTokens) {
                    localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, CompanyId: response.compid, role: response.role, refreshToken: response.refresh_token, userid: response.userid, Warehouseid: response.Warehouseid, useRefreshTokens: true, PagePermission: response.pagePermissions });
                }
                else {

                    localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, CompanyId: response.compid, role: response.role, refreshToken: "", userid: response.userid, Warehouseid: response.Warehouseid, useRefreshTokens: false, PagePermission: response.pagePermissions });
                }
                _authentication.isAuth = true;
                _authentication.userName = loginData.userName;
                _authentication.role = response.role;
                _authentication.rolenames = response.rolenames;
                _authentication.userid = response.userid;
                _authentication.Warehouseid = response.Warehouseid;
                _authentication.compid = response.compid;
                _authentication.useRefreshTokens = loginData.useRefreshTokens;
                console.log("Authentication Token")
                console.log(_authentication);
                deferred.resolve(response);
            }).error(function (err, status) {
                console.log("Failed, " + err + ", " + status);
                _logOut();
                deferred.reject(err);
            });
            return deferred.promise;
        };

        var _logOut = function () {

            localStorageService.remove('authorizationData');
            _authentication.isAuth = false;
            _authentication.userName = "";
            _authentication.useRefreshTokens = false;
        };

        var _fillAuthData = function () {

            var authData = localStorageService.get('authorizationData');
            if (authData) {
                _authentication.isAuth = true;
                _authentication.userName = authData.userName;
                _authentication.useRefreshTokens = authData.useRefreshTokens;
            }
        };

        var _refreshToken = function () {

            var deferred = $q.defer();
            var authData = localStorageService.get('authorizationData');
            if (authData) {
                if (authData.useRefreshTokens) {
                    var data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken + "&client_id=" + ngAuthSettings.clientId;
                    localStorageService.remove('authorizationData');
                    $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
                        localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: response.refresh_token, useRefreshTokens: true });
                        deferred.resolve(response);
                    }).error(function (err, status) {
                        _logOut();
                        deferred.reject(err);
                    });
                }
            }
            return deferred.promise;
        };

        var _obtainAccessToken = function (externalData) {
            var deferred = $q.defer();

            $http.get(serviceBase + 'api/account/ObtainLocalAccessToken', { params: { provider: externalData.provider, externalAccessToken: externalData.externalAccessToken } }).success(function (response) {

                localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: "", useRefreshTokens: false });

                _authentication.isAuth = true;
                _authentication.userName = response.userName;
                _authentication.useRefreshTokens = false;
                deferred.resolve(response);

            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });
            return deferred.promise;
        };

        var _registerExternal = function (registerExternalData) {

            var deferred = $q.defer();
            $http.post(serviceBase + 'api/account/registerexternal', registerExternalData).success(function (response) {
                localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: "", useRefreshTokens: false });

                _authentication.isAuth = true;
                _authentication.userName = response.userName;
                _authentication.useRefreshTokens = false;
                deferred.resolve(response);
            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });
            return deferred.promise;
        };

        authServiceFactory.saveRegistration = _saveRegistration;
        authServiceFactory.saveRegistrationpeople = _saveRegistrationpeople;
        authServiceFactory.login = _login;
        authServiceFactory.logOut = _logOut;
        authServiceFactory.fillAuthData = _fillAuthData;
        authServiceFactory.authentication = _authentication;
        authServiceFactory.refreshToken = _refreshToken;
        authServiceFactory.isAdmin = _isAdmin;
        authServiceFactory.obtainAccessToken = _obtainAccessToken;
        authServiceFactory.externalAuthData = _externalAuthData;
        authServiceFactory.registerExternal = _registerExternal;

        return authServiceFactory;

        return service;

        function getData() { }
    }
})();