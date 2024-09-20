
(function () {
    'use strict';

    angular
        .module('app')
        .controller('roleController', roleController);

    roleController.$inject = ['$scope', 'RoleService', "$filter", "$http", "ngTableParams", '$modal'];

    function roleController($scope, RoleService, $filter, $http, ngTableParams, $modal) {

        console.log(" Role Controller reached");

        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened Role");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myroleModal.html",
                    controller: "ModalInstanceCtrlrole", resolve: { role: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {


                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };


        $scope.edit = function (item) {
            console.log("Edit Dialog called ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myroleModalPut.html",
                    controller: "ModalInstanceCtrlrole", resolve: { role: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.roles.push(selectedItem);
                    _.find($scope.roles, function (role) {
                        if (role.id == selectedItem.id) {
                            role = selectedItem;
                        }
                    });

                    $scope.roles = _.sortBy($scope.role, 'Id').reverse();
                    $scope.selected = selectedItem;

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };

        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for role");



            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleterole.html",
                    controller: "ModalInstanceCtrldeleterole", resolve: { role: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };

        $scope.roles = [];

        RoleService.getRoles().then(function (results) {

            $scope.roles = results.data;


            $scope.callmethod();
        }, function (error) {

        });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.roles;

                $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
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
            $scope.select(1); $scope.currentPage = 1, $scope.row = "";
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
        .controller('ModalInstanceCtrlrole', ModalInstanceCtrlrole);

    ModalInstanceCtrlrole.$inject = ["$scope", '$http', 'ngAuthSettings', "RoleService", "$modalInstance", "role", 'FileUploader'];

    function ModalInstanceCtrlrole($scope, $http, ngAuthSettings, RoleService, $modalInstance, role, FileUploader) {
        console.log("role");

        var input = document.getElementById("file");
        console.log(input);

        var today = new Date();
        $scope.today = today.toISOString();

        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });

        $scope.uploadedfileName = '';
        $scope.upload = function (files) {
            console.log(files);
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    console.log(config.file.name);

                    console.log("File Name is " + $scope.uploadedfileName);
                    //var fileuploadurl = '/api/upload/post', files;
                    var fileuploadurl = '/api/upload/post';
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





        $scope.RoleData = {

        };
        if (role) {
            console.log("role if conditon");

            $scope.RoleData = role;

            console.log($scope.RoleData.RoleTitle);

        }


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.Addrole = function (data) {


                console.log("rolesssssss");

                //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
                //console.log(FileUrl);
                //console.log("Image name in Insert function :" + $scope.uploadedfileName);
                //$scope.AssetsCategoryData.FileUrl = FileUrl;
                //console.log($scope.AssetsCategoryData.FileUrl);




                var url = serviceBase + "api/Roles";
                var dataToPost = { RoleTitle: $scope.RoleData.RoleTitle, CreatedDate: $scope.RoleData.CreatedDate, UpdatedDate: $scope.RoleData.UpdatedDate, CreatedBy: $scope.RoleData.CreatedBy, UpdateBy: $scope.RoleData.UpdateBy };
                console.log(dataToPost);

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



        $scope.Putrole = function (data) {
            $scope.RoleData = {

            };
            if (role) {
                $scope.RoleData = role;
                console.log("found Puttt role");
                console.log(role);
                console.log($scope.RoleData.RoleId);

            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                console.log("Update ");

            //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            //console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);

            var url = serviceBase + "api/Roles";
            var dataToPost = { RoleId: $scope.RoleData.RoleId, RoleTitle: $scope.RoleData.RoleTitle, CreatedDate: $scope.RoleData.CreatedDate, UpdatedDate: $scope.RoleData.UpdatedDate, CreatedBy: $scope.RoleData.CreatedBy, UpdateBy: $scope.RoleData.UpdateBy };
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

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
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })

        };


        /////////////////////////////////////////////////////// angular upload code


        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/upload'
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


    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleterole', ModalInstanceCtrldeleterole);

    ModalInstanceCtrldeleterole.$inject = ["$scope", '$http', "$modalInstance", "RoleService", 'ngAuthSettings', "role"];

    function ModalInstanceCtrldeleterole($scope, $http, $modalInstance, RoleService, ngAuthSettings, role) {
        console.log("delete modal opened");

        function ReloadPage() {
            location.reload();
        }

        $scope.roles = [];

        if (role) {
            $scope.RoleData = role;
            console.log("found role");
            console.log(role);
            console.log($scope.RoleData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


            $scope.deleterole = function (dataToPost, $index) {

                console.log("Delete controller");


                RoleService.deleteRole(dataToPost).then(function (results) {
                    console.log("Del");

                    console.log("index of item " + $index);
                    console.log($scope.roles.length);
                    //$scope.role.splice($index, 1);
                    console.log($scope.roles.length);


                    $modalInstance.close(dataToPost);
                    ReloadPage();

                }, function (error) {
                    alert(error.data.message);
                });
            };

    }
})();