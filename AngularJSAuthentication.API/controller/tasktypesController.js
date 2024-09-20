

(function () {
    'use strict';

    angular
        .module('app')
        .controller('tasktypesController', tasktypesController);

    tasktypesController.$inject = ['$scope', 'tasktypesService', "$filter", "$http", "ngTableParams", '$modal', '$rootScope', 'FileUploader'];

    function tasktypesController($scope, tasktypesService, $filter, $http, ngTableParams, $modal, $rootScope, FileUploader) {

        $scope.upload = function (files) {
            console.log("In Task Type upload");
            console.log(files);
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    console.log(config.file.name);

                    console.log("File Name is " + $scope.uploadedfileName);
                    //var fileuploadurl = 'api/projectupload', files;
                    var fileuploadurl = 'api/projectupload';
                    $upload.upload({
                        url: fileuploadurl,
                        method: "POST",
                        data: { fileUploadObj: $scope.fileUploadObj },
                        file: file
                    }).progress(function (evt) {
                        var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                        console.log('progress: ' + progressPercentage + '% ' +
                            evt.config.file.name);
                    }).success(function (data, status, headers, config) {


                        console.log('file ' + config.file.name + 'uploaded. Response: ' +
                            JSON.stringify(data));
                        console.log("uploaded");
                    });
                }
            }
        };


        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/tasktypeupload'
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
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
        };
        uploader.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader.onSuccessItem = function (fileItem, response, status, headers) {
            console.info('onSuccessItem', fileItem, response, status, headers);
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
        };
        uploader.onCompleteAll = function () {
            console.info('onCompleteAll');
        };

        console.info('uploader', uploader);

        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Open Dialog called");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myTaskTypeModal.html",
                    controller: "ModalInstanceCtrl2", resolve: { tasktype: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");

                });
        };




        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Open Dialog called");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myTaskTypeModaldelete.html",
                    controller: "ModalInstanceCtrldelete", resolve: { tasktype: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condintion");
                });
        };


        $scope.edit = function (item) {
            console.log("Open Dialog called");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myTaskTypeModalUpdate.html",
                    controller: "ModalInstanceCtrl2", resolve: { tasktype: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.tasktypes.push(selectedItem);
                _.find($scope.tasktypes, function (tasktype) {
                    if (tasktype.id == selectedItem.id) {
                        tasktype = selectedItem;
                    }
                });

                $scope.tasktypes = _.sortBy($scope.tasktypes, 'Id').reverse();
                $scope.selected = selectedItem;

                //$scope.customers.push(selectedItem);

                //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                //$scope.selected = selectedItem;


            },
                function () {
                    console.log("Cancel Condintion");
                    // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //  $log.info("Modal dismissed at: " + new Date)
                });
        };


        tasktypesService.gettasktypes().then(function (results) {
            console.log("Get Method Called");
            $scope.tasktypes = results.data;
            $scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });

        //$scope.Delete = function (Id) {
        //    console.log("Delete taskType");
        //    alert(Id);
        //    //Data = { Id: Tid };
        //    var promiseDelete = tasktypesService.deletetasktypes(Id);

        //}    


        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.tasktypes,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);

                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrl2', ModalInstanceCtrl2);

    ModalInstanceCtrl2.$inject = ["$scope", '$http', "$modalInstance", "tasktypesService", "tasktype", 'ngAuthSettings'];

    function ModalInstanceCtrl2($scope, $http, $modalInstance, tasktype, ngAuthSettings) {

        $scope.TaskTypeData = {

        };
        if (tasktype) {
            $scope.TaskTypeData = tasktype;
            console.log("found tasktype");
            console.log(tasktype);
            console.log($scope.TaskTypeData);
            //console.log($scope.Customer.name);
            //console.log($scope.Customer.description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.addTaskType = function (data) {
                console.log("add task type called");

                var url = serviceBase + "api/TaskTypes";
                var dataToPost = { id: 0, Name: data.Name, Desc: data.Desc, Category: data.Category };
                console.log(dataToPost);
                ////$("#spinner").show();

                $http.post(url, dataToPost)
                    .success(function (data) {

                        console.log("Error Gor Here");
                        console.log(data);
                        if (data.id == 0) {

                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;
                            }

                        }
                        else {
                            //console.log(data);
                            //  console.log(data);
                            $modalInstance.close(data);
                        }

                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);

                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    })
            };

        $scope.PutTaskType = function (data) {
            $scope.TaskTypeData = {

            };
            if (tasktype) {
                $scope.TaskTypeData = tasktype;
                console.log("found Puttt TaskType");
                console.log(tasktype);
                console.log($scope.TaskTypeData);
                //console.log($scope.Customer.name);
                //console.log($scope.Customer.description);
            }

            console.log("Update TaskType");
            var url = serviceBase + "api/TaskTypes";
            var dataToPost = { Id: $scope.TaskTypeData.Id, Category: $scope.TaskTypeData.Category, Name: $scope.TaskTypeData.Name, Desc: $scope.TaskTypeData.Desc, CreatedDate: $scope.TaskTypeData.CreatedDate, UpdatedDate: $scope.TaskTypeData.UpdatedDate, CreateBy: $scope.TaskTypeData.CreateBy, UpdatedBy: $scope.TaskTypeData.UpdatedBy, user_userId: $scope.TaskTypeData.user_userId };
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                        //console.log(data);
                        //  console.log(data);
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

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldelete', ModalInstanceCtrldelete);

    ModalInstanceCtrldelete.$inject = ["$scope", '$http', "$modalInstance", "tasktypesService", "tasktype", 'ngAuthSettings'];

    function ModalInstanceCtrldelete($scope, $http, $modalInstance, tasktypesService, tasktype, ngAuthSettings) {
        console.log("delete modal opened");

        $scope.TaskTypeData = {

        };
        if (tasktype) {
            $scope.TaskTypeData = tasktype;
            console.log("found tasktype");
            console.log(tasktype);
            console.log($scope.TaskTypeData);
            //console.log($scope.Customer.name);
            //console.log($scope.Customer.description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            //$scope.addTaskType = function (data) {
            //    console.log("add task type called");

            //    var url = serviceBase + "api/TaskTypes";
            //    var dataToPost = { id: 0, Name: data.Name, Desc: data.Desc };
            //    console.log(dataToPost);
            //    ////$("#spinner").show();

            //    $http.post(url, dataToPost)
            //    .success(function (data) {

            //        console.log("Error Gor Here");
            //        console.log(data);
            //        if (data.id == 0) {

            //            $scope.gotErrors = true;
            //            if (data[0].exception == "Already") {
            //                console.log("Got This User Already Exist");
            //                $scope.AlreadyExist = true;
            //            }

            //        }
            //        else {
            //            //console.log(data);
            //            //  console.log(data);
            //            $modalInstance.close(data);
            //        }

            //    })
            //     .error(function (data) {
            //         console.log("Error Got Heere is ");
            //         console.log(data);

            //         // return $scope.showInfoOnSubmit = !0, $scope.revert()
            //     })

            //};


            $scope.deletetasktypes = function (dataToPost, index) {




                console.log("Delete TaskType controller");
                //alert(Id);
                //Id = window.encodeURIComponent(Id);

                tasktypesService.deletetasktypes(dataToPost).then(function (results) {
                    console.log("Del");

                    //$scope.Deletetasktypes.splice(index, 1);

                    $modalInstance.close(dataToPost);

                }, function (error) {
                    alert(error.data.message);
                });
            }

    }
})();


