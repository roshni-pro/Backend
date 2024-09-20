

(function () {
    'use strict';

    angular
        .module('app')
        .controller('projectsController', projectsController);

    projectsController.$inject = ['$scope', 'projectsService', 'customerService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader', '$log'];

    function projectsController($scope, projectsService, customerService, $filter, $http, ngTableParams, $modal, FileUploader, $log) {

        console.log("reached");

        $scope.upload = function (files) {
            console.log(files);
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    console.log(config.file.name);

                    console.log("File Name is " + $scope.uploadedfileName);
                    //var fileuploadurl = '/api/projectupload/post', files;
                    var fileuploadurl = '/api/projectupload/post';
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
            url: serviceBase + '/api/projectupload'
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
        var fd;

        // $scope.UploadAllowed = false;

        $scope.FileSelectionError = false;



        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened");
            console.log("reached");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myProjetModal.html",
                    controller: "ModalInstanceCtrlProject", resolve: { project: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                    //$scope.currentPageStores = _.sortBy($scope.currentPageStores, 'ProjectID').reverse();
                    //$scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                        // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                        $log.info("Modal dismissed at: " + new Date())
                    })
        };


        $scope.edit = function (item) {
            console.log("Open Dialog called");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myProjetModalPut.html",
                    controller: "ModalInstanceCtrlProject", resolve: { project: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.projects.push(selectedItem);
                    _.find($scope.projects, function (project) {
                        if (project.id == selectedItem.id) {
                            project = selectedItem;
                        }
                    });

                    $scope.projects = _.sortBy($scope.projects, 'ProjectId').reverse();
                    $scope.selected = selectedItem;
                    //$scope.customers.push(selectedItem);

                    //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //$scope.selected = selectedItem;


                },
                    function () {
                        console.log("Cancel Condintion");
                        // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                        //  $log.info("Modal dismissed at: " + new Date)
                    })
        };

        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Open Dialog called");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myTaskTypeModaldeleteproject.html",
                    controller: "ModalInstanceCtrldeleteproject", resolve: { project: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {


                    //$scope.tasktypes.push(selectedItem);

                    //_.filter($scope.tasktypes, function (a) {

                    //    if (a.id == selectedItem.id) {

                    //        a.Name = selectedItem.Name;
                    //        a.Desc = selectedItem.Desc;
                    //    }

                    //});

                    //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //$scope.selected = selectedItem;


                },
                    function () {
                        console.log("Cancel Condintion");
                        // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                        //  $log.info("Modal dismissed at: " + new Date)
                    })
        };

        $scope.projects = [];

        projectsService.getprojects().then(function (results) {

            $scope.projects = results.data;
            $scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });
        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.projects,

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
        .controller('ModalInstanceCtrlProject', ModalInstanceCtrlProject);

    ModalInstanceCtrlProject.$inject = ["$scope", '$http', 'ngAuthSettings', "projectsService", 'customerService', "$modalInstance", "project"];

    function ModalInstanceCtrlProject($scope, $http, ngAuthSettings, projectsService, customerService, $modalInstance, project) {
        console.log("Project");
        $scope.customers = [];
        $scope.ProjectData = {

        };
        customerService.getcustomers().then(function (results) {

            $scope.customers = results.data;

        }, function (error) {
            //alert(error.data.message);
        });

        if (project) {
            console.log("Project if conditon");

            $scope.ProjectData = project;

            console.log($scope.ProjectData.ProjectName);
            //console.log($scope.ProjectData.Description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.AddProject = function () {


                console.log("Adding Project");
                console.log($scope.ProjectData);
                var url = serviceBase + "api/Projects";
                var dataToPost = { CustomerId: $scope.ProjectData.CustomerId, ProjectName: $scope.ProjectData.ProjectName, Discription: $scope.ProjectData.Discription, StartDate: $scope.ProjectData.StartDate, EndDate: $scope.ProjectData.EndDate, Budget: $scope.ProjectData.Budget, ConsultantRate: $scope.ProjectData.ConsultantRate, EmpRate: $scope.ProjectData.EmpRate, ApproverName: $scope.ProjectData.ApproverName, ApproverEmail: $scope.ProjectData.ApproverEmail, CreatedDate: $scope.ProjectData.CreatedDate, UpdatedDate: $scope.ProjectData.UpdatedDate, CreatedBy: $scope.ProjectData.CreatedDate, UpdateBy: $scope.ProjectData.UpdateBy };
                console.log(dataToPost);

                $http.post(url, $scope.ProjectData)
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

        $scope.PutProject = function (data) {
            $scope.ProjectData = {

            };
            if (project) {
                $scope.ProjectData = project;
                console.log("found Puttt Project");
                console.log(project);
                console.log($scope.ProjectData);
                //console.log($scope.Customer.name);
                //console.log($scope.Customer.description);
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                console.log("Update Project");
            var url = serviceBase + "api/projects";
            var dataToPost = { CustomerId: $scope.ProjectData.CustomerId, ProjectID: $scope.ProjectData.ProjectID, ProjectName: $scope.ProjectData.ProjectName, Discription: $scope.ProjectData.Discription, StartDate: $scope.ProjectData.StartDate, EndDate: $scope.ProjectData.EndDate, Budget: $scope.ProjectData.Budget, ConsultantRate: $scope.ProjectData.ConsultantRate, EmpRate: $scope.ProjectData.EmpRate, ApproverName: $scope.ProjectData.ApproverName, ApproverEmail: $scope.ProjectData.ApproverEmail, CreatedDate: $scope.ProjectData.CreatedDate, UpdatedDate: $scope.ProjectData.UpdatedDate, CreatedBy: $scope.ProjectData.CreatedBy, UpdateBy: $scope.ProjectData.UpdateBy };
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
        .controller('ModalInstanceCtrldeleteproject', ModalInstanceCtrldeleteproject);

    ModalInstanceCtrldeleteproject.$inject = ["$scope", '$http', "$modalInstance", "projectsService", 'customerService', 'ngAuthSettings', "project"];

    function ModalInstanceCtrldeleteproject($scope, $http, $modalInstance, projectsService, customerService, ngAuthSettings, project) {
        console.log("delete modal opened");

        //$scope.customers = [];

        //customerService.getcustomers().then(function (results) {

        //    $scope.customers = results.data;

        //}, function (error) {
        //    //alert(error.data.message);
        //});
        $scope.ProjectData = {

        };
        if (project) {
            $scope.ProjectData = project;
            console.log("found tasktype");
            console.log(project);
            console.log($scope.ProjectData);
            //console.log($scope.Customer.name);
            //console.log($scope.Customer.description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


            $scope.deleteprojects = function (dataToPost, index) {

                console.log("Delete Project controller");
                //alert(Id);
                //Id = window.encodeURIComponent(Id);

                projectsService.deleteprojects(dataToPost).then(function (results) {
                    console.log("Del");

                    $modalInstance.close(dataToPost);
                    //console.log("index of item " + $index);
                    //console.log($scope.customers.length);
                    //$scope.data.splice($index, 1);
                    //console.log($scope.customers.length);

                    //$scope.customers.splice($index, 1);
                    //$modalInstance.close(dataToPost);

                }, function (error) {
                    alert(error.data.message);
                });
            }

    }
})();