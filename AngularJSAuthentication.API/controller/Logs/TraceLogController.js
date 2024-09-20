'use strict';
app.controller('TraceLogController', ['$scope', '$http', 'ngAuthSettings', 'ngTableParams',  function ($scope, $http, ngAuthSettings, ngTableParams) {
    $scope.CreatedDate = "";   
    $scope.UserName = "";   
    $scope.ApiName = "";
    $scope.CreatedDate = "";   
    $scope.StartTime = "";   
    $scope.EndTime = "";   
    $scope.SearchString = "";
    $scope.CoRelationId = "";
    $scope.RecordCount = 0;
    $scope.currentPage = 1;
    $scope.url = serviceBase + 'api/History/GetTraceHistory';
    $scope.Search = function () {     
        
        if ($scope.CreatedDate == "") {
            alert("Please enter Date");
            return;
        }

        //$scope.dataforsearch.CreatedDate = data.CreatedDate;

        var dataToPost = {
            dateStr: $scope.CreatedDate,
            apiName: $scope.ApiName,
            userName: $scope.UserName,
            startTime: $scope.StartTime,
            endTime: $scope.EndTime,
            MessageSearch: $scope.SearchString,
            CoRelationId: $scope.CoRelationId,
            Skip: 0,
            Take:50
        };
        console.log('resultdsdsdsdsdsds: ');
        $http.post($scope.url, dataToPost)
            .then(function (result) {    
                console.log('result: ', result);
                $scope.TraceLogs = result.data.TraceLogList;                
                $scope.RecordCount = result.data.Count;
                if ($scope.RecordCount == 0) {
                    alert("Logs not found for search");
                }
               
            });        
    }


    $scope.search = function (pageNumber) {
        var dataToPost = {
            dateStr: $scope.CreatedDate,
            apiName: $scope.ApiName,
            userName: $scope.UserName,
            startTime: $scope.StartTime,
            endTime: $scope.EndTime,
            MessageSearch: $scope.SearchString,
            CoRelationId: $scope.CoRelationId,
            Skip: 0,
            Take: 50
        };
        dataToPost.Skip = (pageNumber - 1) * 50;
        $scope.TraceLogs = [];
        $http.post($scope.url, dataToPost)
            .then(function (result) {
                console.log('result: ', result);
                $scope.TraceLogs = result.data.TraceLogList;
                $scope.RecordCount = result.data.Count;
                if ($scope.RecordCount == 0) {
                    alert("Logs not found for search");
                }

            });        
    }

}]);