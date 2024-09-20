

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CustomerVoiceController', CustomerVoiceController);

    CustomerVoiceController.$inject = ['$scope', '$sce', "$filter", "$http", "ngTableParams", "$modal"];

    function CustomerVoiceController($scope, $sce, $filter, $http, ngTableParams, $modal) {
        $scope.getdetail = function () {


            var url = serviceBase + "api/Customervoice/GET";
            $http.get(url)
                .success(function (data) {

                    $scope.Voicedata = data;
                    $scope.oldpords = true;
                })
                .error(function (data) {
                })
        }
        $scope.getdetail();
        $scope.view = function (item) {

            console.log("View Dialog called ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myViewModal.html",
                    controller: "ModalInstanceCtrlViewMessage", resolve: { message: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.messages.push(selectedItem);
                _.find($scope.messages, function (message) {
                    if (message.id == selectedItem.id) {
                        message = selectedItem;
                    }
                });

                $scope.messages = _.sortBy($scope.message, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };
        $scope.reply = function (replydata) {

            console.log("Reply Dialog called ");
            var modalInstance1;
            modalInstance1 = $modal.open(
                {
                    templateUrl: "myReplyModal.html",
                    controller: "ModalInstanceCtrlMessage", resolve: { message: function () { return item } }
                });
            modalInstance1.result.then(function (selectedItem) {

                    $scope.messages.push(selectedItem);
                    _.find($scope.messages, function (message) {
                        if (message.id == selectedItem.id) {
                            message = selectedItem;
                        }
                    });

                    $scope.messages = _.sortBy($scope.message, 'Id').reverse();
                    $scope.selected = selectedItem;

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };
        $scope.callmethod = function () {

            var init;

            $scope.stores = $scope.User;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               
            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }
        $scope.select = function (page) {

            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {

            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {

            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {

            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {

            console.log("search");
            console.log($scope.stores);
            console.log($scope.searchKeywords);

            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {

            console.log("order"); console.log($scope.stores);
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlViewMessage', ModalInstanceCtrlViewMessage);

    ModalInstanceCtrlViewMessage.$inject = ["$scope", '$sce', '$http', 'ngAuthSettings', "$modalInstance", "message", 'FileUploader', "$modal"];

    function ModalInstanceCtrlViewMessage($scope, $sce, $http, ngAuthSettings, $modalInstance, message, FileUploader, $modal) {
        console.log("View");


        $scope.getdetail1 = function (data) {

            if (message) {
                $scope.Voicedata = message;

            }
            var url = serviceBase + "api/Customervoice/GET1?Skcode=" + message;
            $http.get(url)
                .success(function (data) {

                    for (var i = 0; i < data.length; i++) {
                        $scope.audios = data[i].filepath;
                        data[i].filepath = $sce.trustAsResourceUrl($scope.audios);
                    }
                    $scope.Voicedata = data;
                    $scope.oldpords = true;
                })
                .error(function (data) {
                })
        }
        $scope.getdetail1();
        $scope.reply = function (replydata) {

            console.log("Reply Dialog called ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myReplyModal.html",
                    controller: "ModalInstanceCtrlMessage", resolve: { message: function () { return replydata } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.messages.push(selectedItem);
                _.find($scope.messages, function (message) {
                    if (message.id == selectedItem.id) {
                        message = selectedItem;
                    }
                });

                $scope.messages = _.sortBy($scope.message, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlMessage', ModalInstanceCtrlMessage);

    ModalInstanceCtrlMessage.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "message", 'FileUploader'];

    function ModalInstanceCtrlMessage($scope, $http, ngAuthSettings, $modalInstance, message, FileUploader) {
        console.log("reply");

        if (message) {
            $scope.Voicedata = message;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.MessageReply = function (data) {


            if (message) {
                $scope.Voicedata = message;
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update ");

            var url = serviceBase + "api/CustomerVoice/voicereply";
            var dataToPost = {
                skcode: $scope.Voicedata.skcode,
                message: $scope.Voicedata.message
            };
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (data) {

                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })

        };

    }
})();

