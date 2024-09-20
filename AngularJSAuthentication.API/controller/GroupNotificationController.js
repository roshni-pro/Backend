

(function () {
    'use strict';

    angular
        .module('app')
        .controller('GroupNotificationController', GroupNotificationController);

    GroupNotificationController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', "GroupNotificationService"];

    function GroupNotificationController($scope, $filter, $http, ngTableParams, $modal, GroupNotificationService) {

        $scope.currentPageStores = {};
        $scope.Notification = [];

        $scope.stateChanged = function (qId) {
            if ($scope.answers[qId]) { //If it is checked
                alert('test');
            }
        }


        $scope.open = function () {
            var modalInstance;

            modalInstance = $modal.open(
                {

                    templateUrl: "myADDModal.html",
                    controller: "GroupNotificationADDCtrl", resolve: { object: function () { return $scope.items } }
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
                    controller: "GroupNotificationADDCtrl", resolve: { object: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.DiningTables.push(selectedItem);
                _.find($scope.Notification, function (Notification) {
                    if (Notification.id == selectedItem.id) {
                        Notification = selectedItem;
                    }
                });
                $scope.Notification = _.sortBy($scope.Notification, 'Id').reverse();
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
                    controller: "GroupNotificationdeletectrl", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                })

        };

        GroupNotificationService.getgroupNotification().then(function (results) {
            $scope.Notification = results.data;
            $scope.callmethod();
        })


        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.Notification;
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
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            $scope.select(1); $scope.currentPage = 1;
        },

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
        .controller('GroupNotificationADDCtrl', GroupNotificationADDCtrl);

    GroupNotificationADDCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "NotificationService", "CustomerService"];

    function GroupNotificationADDCtrl($scope, $http, ngAuthSettings, $modalInstance, object, NotificationService, CustomerService) {

        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();
        $scope.allTags = [];
        $scope.tags = [];
        //$scope.$watch('files', function () {
        //    $scope.upload($scope.files);
        //});

        //$scope.uploadedfileName = '';
        //$scope.upload = function (files) {
        //    if (files && files.length) {
        //        for (var i = 0; i < files.length; i++) {
        //            var file = files[i];
        //            var fileuploadurl = '/api/SliderUpload/post', files;
        //            $upload.upload({
        //                url: fileuploadurl,
        //                method: "POST",
        //                data: { fileUploadObj: $scope.fileUploadObj },
        //                file: file
        //            }).progress(function (evt) {
        //                var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
        //            }).success(function (data, status, headers, config) {
        //            });
        //        }
        //    }
        //};

        //CustomerService.getcustomers().then(function (results) {

        //    $scope.customers = results.data;

        //})

        CustomerService.getcustomers().then(function (results) {
            $scope.customers = results.data;
            console.log($scope.customers)


            for (var i = 0; i < $scope.customers.length; i++) {
                $scope.allTags.push($scope.customers[i].Fname)
            }

            console.log($scope.allTags);
            console.log("arr")

        });







        $scope.saveData = {};

        if (object) {
            $scope.saveData = object;
        }

        $scope.Notification = [];

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.Add = function (data) {


            var url = serviceBase + "api/GroupNotification";

            $scope.newar = [];

            for (var i = 0; i < $scope.tags.length; i++) {
                for (var j = 0; j < $scope.customers.length; j++) {
                    if ($scope.tags[i] == $scope.customers[j].Fname) {
                        var d = { "id": "", "name": "" }
                        d.id = $scope.customers[j].CustomerId
                        d.name = $scope.customers[j].Fname
                        $scope.newar.push(d);
                    }
                }
            }
            // end for tag
            console.log($scope.newar);
            console.log("arr")

            $scope.Customer = $scope.newar;

            var data = [];
            _.map($scope.Customer, function (obj) {
                data.push({ "CustomerId": obj.id });
            })
            var dataToPost = {
                Customer: data, GroupName: $scope.saveData.GroupName
            };
            console.log(dataToPost);
            $http.post(url, dataToPost).$modalInstance.close(dataToPost);









        };






    }
})();



(function () {
    'use strict';

    angular
        .module('app')
        .controller('GroupNotificationdeletectrl', GroupNotificationdeletectrl);

    GroupNotificationdeletectrl.$inject = ["$scope", '$http', "$modalInstance", "GroupNotificationService", 'ngAuthSettings', "object"];

    function GroupNotificationdeletectrl($scope, $http, $modalInstance, GroupNotificationService, ngAuthSettings, object) {

        $scope.group = [];
        if (object) {
            $scope.saveData = object;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.delete = function (dataToPost, $index) {
            GroupNotificationService.deletegroupNotification(dataToPost).then(function (results) {
                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });
        }


    }
})();








app.directive('tagManager', function () {


    return {
        restrict: 'E',
        scope: {
            tags: '=',
            autocomplete: '=autocomplete'
        },
        template:
            '<div class="tags">' +
            '<div ng-repeat="(idx, tag) in tags" class="tag label label-success">{{tag}} <a class="close" href ng-click="remove(idx)">×</a></div>' +
            '</div>' +
            '<div class="input-group"><input type="text" class="form-control" placeholder="add a tag..." ng-model="newValue" /> ' +
            '<span class="input-group-btn"><a class="btn btn-default" ng-click="add()">Add</a></span></div>',
        link: function ($scope, $element) {

            var input = angular.element($element).find('input');

            // setup autocomplete
            if ($scope.autocomplete) {
                console.log("purvi");
                console.log($scope.autocomplete);
                $scope.autocompleteFocus = function (event, ui) {
                    input.val(ui.item.value);
                    return false;
                };
                $scope.autocompleteSelect = function (event, ui) {
                    $scope.newValue = ui.item.value;
                    $scope.$apply($scope.add);

                    return false;
                };
                $($element).find('input').autocomplete({
                    minLength: 0,
                    source: function (request, response) {
                        var item;
                        return response((function () {
                            var _i, _len, _ref, _results;
                            _ref = $scope.autocomplete;
                            _results = [];
                            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                                item = _ref[_i];

                                if (item != null) {

                                    if (item.toLowerCase().indexOf(request.term.toLowerCase()) !== -1) {
                                        _results.push(item);
                                    }

                                }

                            }
                            return _results;
                        })());
                    },
                    focus: (function (_this) {
                        return function (event, ui) {
                            return $scope.autocompleteFocus(event, ui);
                        };
                    })(this),
                    select: (function (_this) {
                        return function (event, ui) {
                            return $scope.autocompleteSelect(event, ui);
                        };
                    })(this)
                });
            }


            // adds the new tag to the array
            $scope.add = function () {
                // if not dupe, add it
                if ($scope.tags.indexOf($scope.newValue) == -1) {
                    $scope.tags.push($scope.newValue);
                }
                $scope.newValue = "";
            };

            // remove an item
            $scope.remove = function (idx) {
                $scope.tags.splice(idx, 1);
            };

            // capture keypresses
            input.bind('keypress', function (event) {
                // enter was pressed
                if (event.keyCode == 13) {
                    $scope.$apply($scope.add);
                }
            });
        }
    };
})


