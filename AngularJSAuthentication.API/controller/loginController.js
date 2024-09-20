

(function () {
    'use strict';

    angular
        .module('app')
        .controller('loginController', loginController);

    loginController.$inject = ['$scope', '$location', 'authService', 'ngAuthSettings'];

    function loginController($scope, $location, authService, ngAuthSettings) {

        console.log("Called login controller");
        $scope.pageClass = 'page-contact';
        $scope.loginData = {
            userName: "",
            password: "",
            useRefreshTokens: false
        };

        $scope.message = "";

        $scope.login = function () {
            
            localStorage.hasReloaded = false;
            authService.login($scope.loginData).then(function (response) {
                
                var RolePerson = { "token": response.access_token, "Email": response.email, "userName": response.userName, "role": response.role, "userid": response.userid, "Warehouseid": response.Warehouseid };
                localStorage.hasReloaded = false;
                localStorage.setItem('RolePerson', JSON.stringify(response));
                localStorage.hasReloaded = false;
                var warehouseids = response.Warehouseids.split(',').map(Number)
                localStorage.setItem('warehouseids', warehouseids);
                if (response.Active == false) {

                    localStorage.removeItem('RolePerson');
                    $location.path('/pages/signin');

                }
                else {
                   
                    localStorage.setItem('RolePerson', JSON.stringify(response));
                    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
                    if ($scope.UserRole.role == "Supplier") {
                        $location.path('/Promo');
                        location.reload();
                    }
                    else {

                        if ($scope.UserRole.role == "Presta") {
                            localStorage.removeItem('RolePerson');
                            $location.path('/pages/signin');
                            location.reload();
                        }
                        else {
                            //var roles = $scope.UserRole.rolenames.split(',');
                            //var isAdminRole = roles ? roles.filter(x => x == 'HQ Master login' || x =='WH Master login') : null;
                            //if (isAdminRole[0] == 'HQ Master login' || isAdminRole[0] == 'WH Master login' ) {
                            if (parseInt($scope.UserRole.Warehouseid) == 0) {
                                $location.path('/Welcome');
                            } else {
                                if (warehouseids.length > 1) {
                                    var jwtToken = {
                                        "access_token": $scope.UserRole.access_token,
                                        "userid": $scope.UserRole.userid,
                                        "userName": $scope.UserRole.userName,
                                        "Warehouseids": $scope.UserRole.Warehouseids

                                    }
                                    var preURI = saralUIPortal + "token";
                                    var uri = "layout/warehouse-selection"
                                    var redirectURL = uri.replace(/\//g, '---');  //'/layout---Account---ladgerreport/';
                                    //var token = JSON.stringify(jwtToken);
                                    var token = $scope.UserRole.access_token;
                                    var Warehouseids = $scope.UserRole.Warehouseids;
                                    var userid = $scope.UserRole.userid;
                                    var userName = $scope.UserRole.userName;
                                    var Warehouseid = $scope.UserRole.Warehouseid;
                                    window.location.replace(preURI + "/" + redirectURL + "/" + token + "/" + Warehouseids + "/" + userid + "/" + userName + "/" + Warehouseid);
                                } else {
                                    $location.path('/Welcome');
                                }
                            }
                            // $location.reload();////it was removeddue to login page call again & aigain

                        }
                    }
                }
                //else {
                //    localStorage.setItem('RolePerson', JSON.stringify(response));
                //    $location.path('/DashboardReport');
                //    location.reload();
                //}
            },
                function (err) {
                    $scope.message = err.error_description;
                });
        };

        $scope.authExternalProvider = function (provider) {

            var redirectUri = location.protocol + '//' + location.host + '/authcomplete.html';

            //var externalProviderUrl = ngAuthSettings.apiServiceBaseUri + "api/Account/ExternalLogin?provider=" + provider 
            //    + "&response_type=token&client_id=" + ngAuthSettings.clientId
            //    + "&redirect_uri=" + redirectUri;
            var externalProviderUrl = ngAuthSettings.apiServiceBaseUri + "api/Account/ExternalLogin?provider=" + provider + "&response_type=token&client_id=" + ngAuthSettings.clientId + "&redirect_uri=" + redirectUri;
            window.$windowScope = $scope;

            var oauthWindow = window.open(externalProviderUrl, "Authenticate Account", "location=0,status=0,width=600,height=750");
        };

        $scope.authCompletedCB = function (fragment) {

            $scope.$apply(function () {

                if (fragment.haslocalaccount == 'False') {

                    authService.logOut();

                    authService.externalAuthData = {
                        provider: fragment.provider,
                        userName: fragment.external_user_name,
                        externalAccessToken: fragment.external_access_token
                    };

                    $location.path('/associate');

                }
                else {
                    //Obtain access token and redirect to orders
                    var externalData = { provider: fragment.provider, externalAccessToken: fragment.external_access_token };
                    authService.obtainAccessToken(externalData).then(function (response) {

                        $location.path('/orders');

                    },
                        function (err) {
                            $scope.message = err.error_description;
                        });
                }
            });
        };
    }
})();
