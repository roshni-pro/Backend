

(function () {
    'use strict';

    angular
        .module('app')
        .controller('SuggetionContoller', SuggetionContoller);

    SuggetionContoller.$inject = ['$scope','$http'];

    function SuggetionContoller($scope, $http) {
        $scope.request = [];
        var url = serviceBase + "api/request";
        $http.get(url)
            .success(function (data) {
                if (data.length === 0)
                    alert("Not Found");
                else
                    $scope.request = data;
                console.log(data);
            });
    }
})();
