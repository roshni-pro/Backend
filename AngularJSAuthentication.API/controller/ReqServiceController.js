
(function () {
    'use strict';

    angular
        .module('app')
        .controller('ReqServiceController', ReqServiceController);

    ReqServiceController.$inject = ['$scope', '$http'];

    function ReqServiceController($scope, $http) {

        $scope.requestService = [];

        var url = serviceBase + "/api/ReqService/get";
        $http.get(url)
            .success(function (data) {
                if (data.length == 0)
                    alert("Not Found");
                else
                    $scope.requestService = data;
                console.log(data);
            });

    }
})();
