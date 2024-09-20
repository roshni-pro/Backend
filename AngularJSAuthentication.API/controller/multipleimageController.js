

(function () {
    'use strict';

    angular
        .module('app')
        .controller('multipleimageController', multipleimageController);

    multipleimageController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal'];

    function multipleimageController($scope, $filter, $http, ngTableParams, $modal) {

        console.log("  Controller reached");
        $scope.filenamesss = [];
        // GET THE FILE INFORMATION.
        $scope.getFileDetails = function (e) {
            $scope.files = [];
            $scope.$apply(function () {
                // STORE THE FILE OBJECT IN AN ARRAY.
                for (var i = 0; i < e.files.length; i++) {
                    $scope.files.push(e.files[i]);
                    console.log(e.files[i].name);
                }
            });
        };

        // NOW UPLOAD THE FILES.
        $scope.uploadFiles = function () {
            var data = new FormData();
            for (var i in $scope.files) {
                data.append("uploadedFile", $scope.files[i]);
            }
            // ADD LISTENERS.
            var objXhr = new XMLHttpRequest();
            objXhr.addEventListener("progress", updateProgress, false);
            objXhr.addEventListener("load", transferComplete, false);
            // SEND FILE DETAILS TO THE API.
            objXhr.open("POST", "/api/Multipleimg/");
            objXhr.send(data);
        }
        // UPDATE PROGRESS BAR.
        function updateProgress(e) {
            if (e.lengthComputable) {
                document.getElementById('pro').setAttribute('value', e.loaded);
                document.getElementById('pro').setAttribute('max', e.total);
            }
        }
        // CONFIRMATION.
        function transferComplete(e) {
            alert("Files uploaded successfully.");
        }   
    }
})();