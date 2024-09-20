
(function () {
    'use strict';
    
    angular
        .module('app')
        .controller('LoginTokenController', LoginTokenController);

    LoginTokenController.$inject = ['$scope', '$routeParams', '$location', 'localStorageService'];

    function LoginTokenController($scope, $routeParams, $location, localStorageService) {
        
        $scope.redirecURL = '';
        var param1 = $routeParams.token;
        var param2 = $routeParams.redirectto;
        var param3 = $routeParams.warehouseids;
        var param4 = $routeParams.userid;
        var param5 = $routeParams.userName;
        var param6 = $routeParams.rolenames;
        var param7 = $routeParams.Warehouseid;
        var redirecURL = $routeParams.redirectto.replace(/---/g, '/');
        var newToken = {
            "access_token": param1,
            "Warehouseids": param3,
            "userid": param4,
            "userName": param5,
            "rolenames": param6,
            "Warehouseid" : param7
        }
        var AuthoriseToken = {
        "token": param1,
        "Warehouseids": param3,
        "userid": param4,
        "userName": param5,
        "rolenames": param6,
        "Warehouseid": param7
    }
        localStorage.setItem('RolePerson', JSON.stringify(newToken));
        localStorage.setItem('warehouseids', param3);
        localStorage.setItem('ls.authorizationData', JSON.stringify(AuthoriseToken));
        setTimeout(function () { $location.path('/' + redirecURL); }, 300);
        
    }
})();