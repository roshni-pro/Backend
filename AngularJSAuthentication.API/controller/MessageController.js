

(function () {
    'use strict';

    angular
        .module('app')
        .controller('MessageController', MessageController);

    MessageController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', "MessageService"];

    function MessageController($scope, $filter, $http, ngTableParams, $modal, MessageService) {

        $scope.currentPageStores = {};
        $scope.ItemGroups = [];

        $scope.open = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "MessageADDCtrl", resolve: { object: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.currentPageStores.push(selectedItem);
            },
                function () {

                })
        };

        $scope.edit = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myputmodal.html",
                    controller: "MessageADDCtrl", resolve: { object: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.Messages.push(selectedItem);
                _.find($scope.Messages, function (ItemGroups) {
                    if (ItemGroups.id == selectedItem.id) {
                        ItemGroups = selectedItem;
                    }
                });

                $scope.Messages = _.sortBy($scope.Messages, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {

                })
        };

        $scope.opendelete = function (data, $index) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mydeletemodal.html",
                    controller: "Messagedeletectrl", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                if (selectedItem == null) {
                }
                else { $scope.currentPageStores.splice($index, 1); }

            },
                function () {
                })

        };

        MessageService.getMessages().then(function (results) {
            $scope.Messages = results.data;
            console.log($scope.Messages);
            $scope.callmethod();
        })


        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.Messages;
            $scope.searchKeywords = "";
                $scope.filteredStores = [];
            $scope.row = "";


            $scope.numPerPageOpt = [3, 5, 10, 20, 50];
            $scope.numPerPage = $scope.numPerPageOpt[3];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
        }


        $scope.select = function (page) {
            var end, start;
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            $scope.select(1); $scope.currentPage = 1, $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {
            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('MessageADDCtrl', MessageADDCtrl);

    MessageADDCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object"];

    function MessageADDCtrl($scope, $http, ngAuthSettings, $modalInstance, object) {

        $scope.saveData = {};

        if (object) {
            $scope.saveData = object;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.Add = function (data) {
            $scope.viewdata = [];
            $scope.viewdata.MessageType = $scope.saveData.MessageType;
            $scope.viewdata.MessageText = $scope.saveData.MessageText;
            $scope.viewdata.MessageTitle = $scope.saveData.MessageTitle;

            console.log($scope.viewdata);
            var url = serviceBase + "api/MessageApi";

            var dataToPost = { MessageTitle: $scope.saveData.MessageTitle, MessageText: $scope.saveData.MessageText, MessageType: $scope.saveData.MessageType };

            $http.post(url, dataToPost)
                .success(function (data) {
                    $scope.viewdata.CreatedDate = data.CreatedDate;
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $modalInstance.close($scope.viewdata);
                    }
                })
                .error(function (data) {

                })
        };



        $scope.Put = function (data) {
            $scope.saveData = {
            };
            if (object) {
                $scope.saveData = object;
            }
            $scope.viewdata = [];
            $scope.viewdata.MessageType = $scope.saveData.MessageType;
            $scope.viewdata.MessageText = $scope.saveData.MessageText;
            $scope.viewdata.MessageTitle = $scope.saveData.MessageTitle;

            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


            var url = serviceBase + "api/MessageApi";
            var dataToPost = { MessageTitle: $scope.saveData.MessageTitle, MessageText: $scope.saveData.MessageText, MessageId: $scope.saveData.MessageId, MessageType: $scope.saveData.MessageType };
            $http.put(url, dataToPost)
                .success(function (data) {
                    if (data.id == 0) {
                        $scope.viewdata.CreatedDate = data.CreatedDate;
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }

                    else {
                        $modalInstance.close($scope.viewdata);
                    }
                })
                .error(function (data) {
                })


        };



    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('Messagedeletectrl', Messagedeletectrl);

    Messagedeletectrl.$inject = ["$scope", '$http', "$modalInstance", "MessageService", 'ngAuthSettings', "object"];

    function Messagedeletectrl($scope, $http, $modalInstance, MessageService, ngAuthSettings, object) {

        $scope.categorys = [];
        if (object) {
            $scope.saveData = object;

        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.delete = function (dataToPost, $index) {
            MessageService.deleteMessage(dataToPost).then(function (results) {
                if (results.data == '"error"') {
                    alert("Item Cannot Be Deleted As It Is Associated With Some Category!");
                    $modalInstance.close(null);
                    return false;
                } else if (results.data == '"success"') {
                    $modalInstance.close(dataToPost);
                }

            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();