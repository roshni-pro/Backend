

(function () {
    'use strict';

    angular
        .module('app')
        .controller('NotificationByDeviceIdController', NotificationByDeviceIdController);

    NotificationByDeviceIdController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', "NotificationByDeviceIdService"];

    function NotificationByDeviceIdController($scope, $filter, $http, ngTableParams, $modal, NotificationByDeviceIdService) {

        $scope.currentPageStores = {};
        $scope.NotificationByDeviceId = [];

        $scope.stateChanged = function (qId) {
            if ($scope.answers[qId]) { //If it is checked
                alert('test');
            }
        }

        $scope.acust = function () {
            var url = serviceBase + "api/Notification/all";
            $http.get(url).success(function (response) {
                $scope.cust = response;
            });
        }
        $scope.acust();


        $scope.currentPageStores = {};
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.itemsPerPage = 50; //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [50, 100, 200, 300];//dropdown options for no. of Items per page
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selected;
            $scope.getNotificationdata($scope.pageno);
        }
        $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown
        $scope.$on('$viewContentLoaded', function () {
            //$scope.getNotificationdata($scope.pageno);
        });
        $scope.getNotificationdata = function (pageno) {

            $scope.currentPageStoress = {};
            var url = serviceBase + "api/Notification/getall" + "?list=" + $scope.itemsPerPage + "&page=" + $scope.pageno + "&customerid=" + $scope.CustomerId;
            $http.get(url)
                .success(function (results) {
                    $scope.currentPageStoress = results.notificationmaster;
                    $scope.total_count = results.total_count;
                })
                .error(function (data) {
                    console.log(data);
                })
        };




        $scope.open = function () {
            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "NotificationByDeviceIdADDCtrl", resolve: { object: function () { return $scope.items } }
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
                    controller: "NotificationByDeviceIdADDCtrl", resolve: { object: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.DiningTables.push(selectedItem);
                _.find($scope.NotificationByDeviceId, function (NotificationByDeviceId) {
                    if (NotificationByDeviceId.id == selectedItem.id) {
                        NotificationByDeviceId = selectedItem;
                    }
                });
                $scope.NotificationByDeviceId = _.sortBy($scope.NotificationByDeviceId, 'Id').reverse();
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
                    controller: "NotificationByDeviceIddeletectrl", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                })
        };


        $scope.currentPageStores = {};
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.itemsPerPage = 20; //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [20, 50, 100, 200, 300];//dropdown options for no. of Items per page
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selected;
            $scope.getnotificationbyId($scope.pageno);
        }
        $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown

        $scope.$on('$viewContentLoaded', function () {
            $scope.getnotificationbyId($scope.pageno);
        });

        $scope.getnotificationbyId = function (pageno) {
            $scope.currentPageStores = {};
            var url = serviceBase + "api/NotificationByDeviceId" + "?list=" + $scope.itemsPerPage + "&page=" + pageno;

            $http.get(url)
                .success(function (results) {
                    $scope.currentPageStores = results.notidevice;
                    $scope.total_count = results.total_count;
                })
                .error(function (data) {
                    console.log(data);
                })
        };



        //NotificationByDeviceIdService.getNotificationByDeviceId().then(function (results) {
        //$scope.NotificationByDeviceId = results.data;
        // $scope.callmethod();
        //})
        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.NotificationByDeviceId;
                $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [3, 5, 10, 20, 50];
            $scope.numPerPage = $scope.numPerPageOpt[3];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
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
        .controller('NotificationByDeviceIdADDCtrl', NotificationByDeviceIdADDCtrl);

    NotificationByDeviceIdADDCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "FileUploader", "NotificationByDeviceIdService", "DeviceNotificationService"];

    function NotificationByDeviceIdADDCtrl($scope, $http, ngAuthSettings, $modalInstance, object, NotificationByDeviceIdService, DeviceNotificationService, FileUploader) {

        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();

        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });

        $scope.uploadedfileName = '';
        $scope.upload = function (files) {
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    //var fileuploadurl = '/api/logoUpload/post', files;
                    var fileuploadurl = '/api/logoUpload/post';
                    $upload.upload({
                        url: fileuploadurl,
                        method: "POST",
                        data: { fileUploadObj: $scope.fileUploadObj },
                        file: file
                    }).progress(function (evt) {
                        var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                    }).success(function (data, status, headers, config) {
                    });
                }
            }
        };

        //CustomerService.getcustomers().then(function (results) {
        //    $scope.customers = results.data;
        //})

        DeviceNotificationService.getDeviceNotification().then(function (results) {
            $scope.deviceNotification = results.data;
            console.log("$scope.deviceNotification", $scope.deviceNotification);
        })

        //NotificationByDeviceIdService.getNotificationByDeviceId().then(function (results) {
        //    $scope.NotificationByDeviceId = results.data;
        //    $scope.callmethod();
        //})



        $scope.saveData = {};

        if (object) {
            $scope.saveData = object;
        }

        $scope.NotificationByDeviceId = [];

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.Add = function (data) {

            var LogoUrl = $scope.uploadedfileName;
            $scope.saveData.ImageUrl = LogoUrl;

            var url = serviceBase + "api/NotificationByDeviceId";
            var dataToPost = {
                "Message": $scope.saveData.Message,
                "NotifiedTo": $scope.saveData.NotifiedTo,
                "NotificationByDeviceIdTime": $scope.saveData.NotificationByDeviceIdTime,
                "ImageUrl": $scope.saveData.ImageUrl,
                "Title": $scope.saveData.Title
            };
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (data) {
                    $modalInstance.close(data);
                })
                .error(function (data) {
                })
        };


        var uploader = $scope.uploader = new FileUploader({
            url: 'api/logoUpload'
        });
        //FILTERS
        uploader.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        //CALLBACKS
        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploader.onAfterAddingFile = function (fileItem) {
            console.log(fileItem);
        };
        uploader.onAfterAddingAll = function (addedFileItems) {
        };
        uploader.onBeforeUploadItem = function (item) {
            console.log(item);
        };
        uploader.onProgressItem = function (fileItem, progress) {
        };
        uploader.onProgressAll = function (progress) {
        };
        uploader.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            $scope.uploadedfileName = fileItem._file.name;
        };
        uploader.onCompleteAll = function () {
            alert("Image Uploaded Successfully");

        };

    }
})();
