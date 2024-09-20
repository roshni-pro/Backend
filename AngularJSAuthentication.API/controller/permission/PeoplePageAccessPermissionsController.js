

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PeoplePageAccessPermissionsController', PeoplePageAccessPermissionsController);

    PeoplePageAccessPermissionsController.$inject = ['$scope', "$filter", "$http", '$modal', 'peoplesService'];

    function PeoplePageAccessPermissionsController($scope, $filter, $http, $modal, peoplesService) {
      //  
        $scope.getPeoples = function () {
            var url = serviceBase + 'api/GrowthModule/GetPeople';
            $http.get(url)
                .success(function (response) {
                    $scope.Peoples = response.peopleslist;
                });
        };
        $scope.getPeoples();


        $scope.Pages = [];
        $scope.getPages = function () {
            var url = serviceBase + "api/PageMaster/GetAllPagesForDropDown";
            $http.get(url).success(function (results) {
                $scope.Pages = results;
                if ($scope.Pages == null) {
                    alert("Data Not Found");
                }
            });
        };

        $scope.getPages();

        $scope.Buttons = [];
        $scope.getButtons = function (id, peopleid) {
            var url = serviceBase + "api/PageMaster/GetPageButtonAccess?peopleId=" + peopleid + "&pageMasterId=" + id;
            $http.get(url).success(function (results) {
                $scope.Buttons = results;
                if ($scope.Buttons == null) {
                    alert("Data Not Found");
                }
            });
        };

        $scope.SavePageButtonPermission = function (permission, pid) {
            var url = serviceBase + "api/PageMaster/SavePeoplePageButtonPermission";

            var datatopost = [];
            for (var i = 0; i < permission.length; i++) {
                var post = {
                    PageButtonId: permission[i].PageButtonId,
                    PeopleId: pid,
                    IsActive: permission[i].IsActive
                };
                datatopost.push(post);
            }

            $http.post(url, datatopost).success(function (results) {
                alert("Record Successfully Saved");
                //window.location.reload();
            });
        };


        $scope.removeSelection = function () {
            $scope.selectedPeople = null;
        };

        //for auto selection
        $scope.selectedData = [];

        $scope.onSelect = function (selection) {
            $scope.selectedPeople = selection;
            var url = serviceBase + "api/PageMaster/GetPermissionData/" + selection.PeopleID;
            $http.get(url).success(function (response) {

                $scope.selectedData = response;

            });
        };

    }
})();