(function () {
    'use strict';

    angular
        .module('app')
        .controller('GroupSMSController', GroupSMSController);

    GroupSMSController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader', 'GroupSmsService'];

    function GroupSMSController($scope, $filter, $http, ngTableParams, $modal, FileUploader, GroupSmsService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.currentPageStores = {};
        $scope.Group = [];

        $scope.open = function () {
            //
            console.log("Modal opened  ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myGroupSMSModal.html",
                    controller: "ModalInstanceCtrlGroupSMS", resolve: { GroupSms: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");


                    })

        };

        $scope.edit = function (egroup) {

            console.log("Edit Dialog called Group");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mygroupPutSMS.html",
                    controller: "ModalInstanceCtrlGroupSMS", resolve: { GroupSms: function () { return egroup } }
                });
            modalInstance.result.then(function (selectedItem) {
                    console.log("Select Group..")
                    $scope.State.push(selectedItem);
                    //_.find($scope.Group, function (State) {
                    //    if (Group.id == selectedItem.id) {
                    //        Group = selectedItem;
                    //    }
                    //});
                    $scope.Group = _.sortBy($scope.Group, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        $scope.opendelete = function (data, $index) {

            console.log(data);
            console.log("Delete Dialog called for Group");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteGroupSMS.html",
                    controller: "ModalInstanceCtrldeleteGroupSMS", resolve: { GroupSms: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };

        GroupSmsService.getgroup().then(function (results) {

            $scope.Group = results.data;
            $scope.callmethod();
        }, function (error) {
        });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.Group;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [10, 50, 100];
            $scope.numPerPage = $scope.numPerPageOpt[0];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
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
        .controller('ModalInstanceCtrlGroupSMS', ModalInstanceCtrlGroupSMS);

    ModalInstanceCtrlGroupSMS.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "GroupSms"];

    function ModalInstanceCtrlGroupSMS($scope, $http, ngAuthSettings, $modalInstance, GroupSms) {


        //Duplicacy Check
        $scope.CheckDuplicacy = function (data) {
            
            var url = serviceBase + "api/GroupSMS/Duplicacy?GroupName=" + data.GroupName + "&GroupAssociation=" + data.GroupAssociation;
            $http.get(url)
                .success(function (data) {
                    if (data == 'null') {
                    }
                    else {
                        alert("Group " + data.GroupName + " already Exist");
                        return;
                        //$scope.PeopleData.Mobile = '';
                    }
                    console.log(data);
                });
        }


        $scope.GroupData = {};
        if (GroupSms) {
            $scope.GroupData = GroupSms;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddGroup = function (data) {
           
            var url = serviceBase + "api/GroupSMS/add";
            var dataToPost = {

                GroupName: data.GroupName,
                GroupAssociation: data.GroupAssociation
            };
            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log(data);
                    console.log($scope.GroupData);
                    console.log("Error Got Here");
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
                        window.location.reload();
                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };

        $scope.putGroup = function (data) {

            var dataToPost = {
                GroupID: data.GroupID,
                GroupName: data.GroupName,
                GroupAssociation: data.GroupAssociation
            };
            var url = serviceBase + "api/GroupSms/put";
            console.log(dataToPost);
            console.log("In put");
            $http.put(url, dataToPost).success(function (data) {
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
                    window.location.reload();
                }
            })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };
    }
})();

(function () {
    
    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteGroupSMS', ModalInstanceCtrldeleteGroupSMS);

    ModalInstanceCtrldeleteGroupSMS.$inject = ["$scope", '$http', "$modalInstance", "GroupSmsService", 'ngAuthSettings', "GroupSms"];

    function ModalInstanceCtrldeleteGroupSMS($scope, $http, $modalInstance, GroupSmsService, ngAuthSettings, GroupSms) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        }
        $scope.GroupSms = [];
        if (GroupSms) {
            $scope.GroupData = GroupSms;
            console.log("found group");
            //console.log(QuesAnsData);
            console.log($scope.GroupData);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.deleteGroup = function (dataToPost, $index) {

            console.log(dataToPost);
            console.log("Delete  warehouse  controller");

            GroupSmsService.deletegroup(dataToPost).then(function (results) {
                console.log("Del");
                console.log("index of item " + $index);
                console.log($scope.GroupSms.length);

                $modalInstance.close(dataToPost);
                window.location.reload();
            }, function (error) {
                alert(error.data.message);
            });
        }
    }
})();
