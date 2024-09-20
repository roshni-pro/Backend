

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CustomerIssueController', CustomerIssueController);

    CustomerIssueController.$inject = ['$scope', "$http", "$modal", "CustomerIssuesService"];

    function CustomerIssueController($scope, $http, $modal, CustomerIssuesService) {
        $scope.CustomerIssues = [];
        CustomerIssuesService.getcustomerissues().then(function (result) {
            $scope.CustomerIssues = result.data;

        });
        $scope.open = function () {

            console.log('model opened Customer')
            var modalinstance;
            modalinstance = $modal.open({
                templateUrl: "myCustomerissuesModel.html",
                controller: "ModelInstanceCtrlCustomerissue", resolve: { Object: function () { return $scope.item } }
            });
        };

        $scope.edit = function (data) {

            console.log('model opened Customer')
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerissuesUpdateModel.html",
                    controller: "ModelInstanceCtrlCustomerissue", resolve: { Object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            })
        };
        $scope.opendelete = function (data, $index) {
            var myData = data;

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteissue.html",
                    controller: "ModelInstanceCtrlCustomerissue", resolve: { Object: function () { return myData } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.CustomerIssues.splice($index, 1);
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
        .controller('ModelInstanceCtrlCustomerissue', ModelInstanceCtrlCustomerissue);

    ModelInstanceCtrlCustomerissue.$inject = ["$scope", '$http', "$modalInstance", 'Object', 'customerService', 'peoplesService', 'CustomerIssuesService'];

    function ModelInstanceCtrlCustomerissue($scope, $http, $modalInstance, Object, customerService, peoplesService, CustomerIssuesService) {

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('cancel'); };
        $scope.customers = [];
        $scope.Search = function (data) {

            customerService.getfilteredCustomer(data).then(function (result) {
                $scope.customers = result.data;
            });
        }

        $scope.peoples = [];
        peoplesService.getpeoples().then(function (result) {

            $scope.peoples = result.data;
        });

        $scope.Submit = function (data) {

            var dataToPost = {
                CustomerId: data.CustomerId,
                PeopleID: data.PeopleID,
                Issue: data.Issue,
                CompletionDate: data.CompletionDate,
            };
            CustomerIssuesService.PostCustomerissue(dataToPost).then(function (result) {
                $modalInstance.close();
            });
        }
        $scope.data = {};
        if (Object) {
            $scope.data = Object;
        }
        $scope.status = [{ name: "Pending" }, { name: "Process" }, { name: "Completed" }];

        $scope.Update = function (data) {

            var dataToPost = {
                CS_id: $scope.data.CS_id,
                Status: $scope.data.Status,
                Issue: $scope.data.Issue
            };
            CustomerIssuesService.PutCustomerissues(dataToPost).then(function (result) {
                $modalInstance.close();
            });
        }

        $scope.deleteIssue = function (data) {

            CustomerIssuesService.deletecustomerissues($scope.data).then(function (result) {
                $modalInstance.close();
            });
        }

    }
})();
