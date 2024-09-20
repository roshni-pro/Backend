

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DeletepeoplesController', DeletepeoplesController);

    DeletepeoplesController.$inject = ['$scope', 'peoplesService', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService', 'ClusterService'];

    function DeletepeoplesController($scope, peoplesService, CityService, StateService, $filter, $http, ngTableParams, $modal, WarehouseService, ClusterService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        console.log("People Controller reached");
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
       {
            //.................File Uploader method start..................    


            //----- Function for Getting Deleted data 
            ///---- By Sudhir -----19/04/2019
            $scope.getremovedpeoples = function () {

                var url = serviceBase + 'api/Peoples/getremovedpeople';
                $http.get(url).success(function (results) {
                    $scope.Peopleremoveddata = results;
                    $scope.callmethod();
                    if (Peopleremoveddata == null) {
                        alert(" Data Not Found");
                    }
                })
            }
            $scope.getremovedpeoples();


            //Get DeletedHistory data function//--------By Sudhir--------19/04/2019
            $scope.PeopleHistroydata = function (data) {

                $scope.dataPeopleHistrorydata = [];
                var url = serviceBase + "api/Peoples/DeletedHistorydata?PeopleId=" + data.PeopleID;
                $http.get(url).success(function (response) {

                    $scope.dataPeopleHistrorydata = response;
                    console.log($scope.dataPeopleHistrorydata);

                })
                    .error(function (data) {
                    })
            }
            // Function for Re-active People after Deleting //--------By Sudhir--------19/04/2019
            $scope.active = function (data) {
               // 
                var url = serviceBase + 'api/Peoples/Undeleted/?peopleid=' + data.PeopleID;
                $http.delete(url).success(function (data) {
                    alert("People Activation Successfull");
                });

                //alert("People Activation Successfull");
                window.location.reload();
            };

            $scope.currentPageStores = {};

            $scope.callmethod = function () {

                var init;
                //return $scope.stores = $scope.Peopleremoveddata,
                $scope.stores = $scope.Peopleremoveddata,

                    $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";

                   

                $scope.numPerPageOpt = [20, 30, 50, 200];
                $scope.numPerPage = $scope.numPerPageOpt[1];
                $scope.currentPage = 1;
                $scope.currentPageStores = [];
                    //(init = function () {
                    //    return $scope.search(), $scope.select($scope.currentPage)
                    //})
                $scope.search(); $scope.select(1)
            }
            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                start = (page - 1) * $scope.numPerPage;
                end = start + $scope.numPerPage;
                $scope.currentPageStores = $scope.filteredStores.slice(start, end)
            }

            $scope.onFilterChange = function () {
                console.log("onFilterChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1; $scope.row = "";
            };

            $scope.onNumPerPageChange = function () {
                console.log("onNumPerPageChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1;
            };

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1;
            };

            $scope.search = function () {
                console.log("search");
                console.log($scope.stores);
                console.log($scope.searchKeywords);

                $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange();
            },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
                };
        }
    }
})();

