


(function () {
    'use strict';

    angular
        .module('app')
        .controller('designationController', designationController);

    designationController.$inject = ['$scope', 'designationservice', "$filter", "$http", "ngTableParams", '$modal'];

    function designationController($scope, designationservice, $filter, $http, ngTableParams, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        //  designationservice'
        console.log(" Designation Controller reached");

        $scope.currentPageStores = {};

        $scope.open = function () {

            console.log("Modal opened Designation");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myDesignationModal.html",
                    controller: "ModalInstanceCtrlDesignation", resolve: { designation: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {


                $scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");
                    window.location.reload();
                })
        };


        $scope.edit = function (item) {

            console.log("Edit Dialog called ");
            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "myDesignationModalPut.html",
                    controller: "ModalInstanceCtrlDesignation", resolve: { designation: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.designations.push(selectedItem);
                _.find($scope.designations, function (designation) {
                    if (designation.id == selectedItem.id) {
                        designation = selectedItem;
                    }
                });

                $scope.designations = _.sortBy($scope.designation, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");
                    window.location.reload();
                })
        };

        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for designation");



            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteDesignation.html",
                    controller: "ModalInstanceCtrldeleteDesignation", resolve: { designation: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                    console.log("Cancel Condintion");
                    window.location.reload();
                })
        };
        $scope.designations = [];


        $http.get(serviceBase + 'api/Designation').then(function (results) {

            if (results.data != "null") {
                $scope.designations = results.data;
                $scope.callmethod();
            }

        }, function (error) {

        });
        //$http.post(serviceBase + 'api/Designations').then(function (results) {
        //    console.log(results);
        //    return results;
        //});


        $scope.callmethod = function () {

            var init;
           $scope.stores = $scope.designations,

                $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

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

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlDesignation', ModalInstanceCtrlDesignation);

    ModalInstanceCtrlDesignation.$inject = ["$scope", '$http', 'ngAuthSettings', "designationservice", "$modalInstance", "designation", 'FileUploader'];

    function ModalInstanceCtrlDesignation($scope, $http, ngAuthSettings, designationservice, $modalInstance, designation, FileUploader) {
        console.log("designation");


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





        $scope.DesignationData = {

        };
        if (designation) {
            console.log("designation if conditon");

            $scope.DesignationData = designation;

            console.log($scope.DesignationData.DesignationName);

        }

        //For Validation 
        $scope.CheckDesignation = function (DesignationName) {

            //var number = document.getElementById("site-name-Designation").value;
            var url = serviceBase + "api/Designation/DesignationName?DesignationName=" + DesignationName;
            $http.get(url)
                .success(function (data) {
                    if (data == 'null') {
                    }
                    else {

                        alert("Name already Exist");
                        // document.getElementById("site-name-Designation").focus();
                        return;
                        //$scope.PeopleData.Name = '';
                    }
                    console.log(data);
                });
        }

        $scope.ok = function () { $modalInstance.close(); window.location.reload() };
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
            window.location.reload();
        };

        $scope.AddDesignation = function (data) {


            console.log("designation");

            //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            //console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);




            var url = serviceBase + "api/Designation";
            var dataToPost = {
                DesignationName: data.DesignationName,
                Description: data.Description,
                Level: data.Level

            };
            console.log(dataToPost);

            $http.post(url, dataToPost)
                .success(function (data) {
                    // 
                    if (data = null || data == "null") {
                        alert("Name already Exist");
                        $modalInstance.close(data);
                        window.location.reload();
                    } else if (data.id == 0) {

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
                        window.location.reload();
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };



        $scope.PutDesignation = function (data) {

            $scope.DesignationData = {

            };

            if (designation) {
                $scope.DesignationData = designation;
                console.log("found Puttt designation");
                console.log(designation);
                console.log($scope.DesignationData);

            }
            $scope.ok = function () { $modalInstance.close(); window.location.reload(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); window.location.reload(); };

            console.log("Update ");

            //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            //console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);

            var url = serviceBase + "api/Designation/cv";
            var dataToPost = {
                Designationid: $scope.DesignationData.Designationid,
                Description: $scope.DesignationData.Description,
                Level: $scope.DesignationData.Level,
                DesignationName: $scope.DesignationData.DesignationName,
                CreatedDate: $scope.DesignationData.CreatedDate,
                UpdatedDate: $scope.DesignationData.UpdatedDate,
                CreatedBy: $scope.DesignationData.CreatedBy,
                UpdateBy: $scope.DesignationData.UpdateBy
            };

            //var dataToPost = { SurveyId: $scope.SurveyData.SurveyId, SurveyCategoryName: $scope.SurveyData.SurveyCategoryName, Discription: $scope.SurveyData.Discription, CreatedDate: $scope.SurveyData.CreatedDate, UpdatedDate: $scope.SurveyData.UpdatedDate, CreatedBy: $scope.SurveyData.CreatedBy, UpdateBy: $scope.SurveyData.UpdateBy };
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

                    // // 
                    // if (data = null || data == "null") {
                    //     alert("Name already Exist");
                    //     $modalInstance.close(data);
                    //     window.location.reload();
                    // }else   if (data.id == 0) {

                    //     $scope.gotErrors = true;
                    //     if (data[0].exception == "Already") {
                    //         console.log("Got This User Already Exist");
                    //         $scope.AlreadyExist = true;
                    //     }

                    // }
                    // else {
                    //     //console.log(data);
                    //     //  console.log(data);
                    //     $modalInstance.close(data);
                    // }

                    // })
                    //window.location.reload();

                    console.log("Error Got Here");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                        window.location.reload();
                    }
                    else {

                        $modalInstance.close(data);
                        window.location.reload();
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
            window.location.reload();
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
        .controller('ModalInstanceCtrldeleteDesignation', ModalInstanceCtrldeleteDesignation);

    ModalInstanceCtrldeleteDesignation.$inject = ["$scope", '$http', 'designationservice', "$modalInstance", 'ngAuthSettings', "designation"];

    function ModalInstanceCtrldeleteDesignation($scope, $http, designationservice, $modalInstance, ngAuthSettings, designation) {
        console.log("delete modal opened");

        $scope.designations = [];

        if (designation) {
            $scope.DesignationData = designation;
            console.log("found designation");
            console.log(designation);
            console.log($scope.DesignationData);

        }
        $scope.ok = function () { $modalInstance.close(); window.location.reload() };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); window.location.reload(); };


        $scope.deletedesignation = function (dataToPost, $index) {

            console.log("Delete  designation controller");
            designationservice.deletedesignation(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                console.log($scope.designations.length);
                console.log($scope.designations.length);

                $modalInstance.close(dataToPost);


            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();
