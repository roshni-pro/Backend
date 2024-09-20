'use strict';
app.controller('UpdateVersionController', ['$scope', "$http",  function ($scope, $http){ 
    
    $scope.UpdateApp = function (data) {
        
        if (!data.version) {
            alert("Please enter version .");
            return false;
        }
        
        var url = serviceBase + "api/Customers/updateversion?apptype=" + data.apptype + '&version=' + data.version;
        var dataToPost = {

            apptype: data.apptype,
            version: data.version

        };
        $http.post(url, dataToPost)
            .success(function (data) {
                
                if (data) {
                    alert("Page save Successfully");
                    window.location.reload();
                }
                else {
                    alert("Some error occurred during save page detail");
                }
            })
            .error(function (data) {
                alert("Some error occurred during save page detail");
            });
        //$modelInstance.close(data);     

    };

   
    //$scope.currentversion = function (data) {
    //    
    //    $scope.getVer = [];
    //    return $http.get(serviceBase + 'api/Customers/currentversion?apptype=' + data.apptype + ' & currentversion=' + data.currentversion).then(function (results) {
    //        $scope.getVer = results.data;
    //    });
    //};
    ////$scope.Currentver = fuction(data) {
    //    
    //    $scope.Currentver = [];

    //    return $http.get(serviceBase + "api/Customers/currentversion?apptype=" + data.apptype + '&currentversion=' + data.currentversion).then(function (results) {

    //        $scope.Currentver = results.data;
    //    });
    //};




    $scope.recent = [];
    $scope.selectever = function (data) {
        
        var url = serviceBase + "api/Customers/currentversion?apptype=" + data;
        $http.get(url)
            .success(function (data) {
                
                $scope.recent = data;
                console.log(data);
            });
    }


}]);