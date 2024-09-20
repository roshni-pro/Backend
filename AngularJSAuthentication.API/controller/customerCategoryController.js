

(function () {
    'use strict';

    angular
        .module('app')
        .controller('customerCategoryController', customerCategoryController);

    customerCategoryController.$inject = ['$scope', 'CustomerCategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function customerCategoryController($scope, CustomerCategoryService, $filter, $http, ngTableParams, $modal) {

        console.log(" Customer Category Controller reached");

        $scope.currentPageStores = {};

        $scope.open = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerCategoryModal.html",
                    controller: "ModalInstanceCtrlAssetsCategory", resolve: { customercategory: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);

            },
                function () {


                })
        };


        $scope.edit = function (item) {
            console.log("Edit Dialog called assets category");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerCategoryModalPut.html",
                    controller: "ModalInstanceCtrlAssetsCategory", resolve: { customercategory: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.assetscategorys.push(selectedItem);
                _.find($scope.assetscategorys, function (assetscategory) {
                    if (assetscategory.id == selectedItem.id) {
                        assetscategory = selectedItem;
                    }
                });

                $scope.assetscategorys = _.sortBy($scope.assetscategorys, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {

                })
        };

        $scope.opendelete = function (data, $index) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteCustomerCategory.html",
                    controller: "ModalInstanceCtrldeleteAssetsCategory", resolve: { customercategory: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {

                })
        };

        $scope.customercategorys = [];

        CustomerCategoryService.getcustomercategorys().then(function (results) {

            $scope.customercategorys = results.data;


            $scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.customercategorys;

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
        .controller('ModalInstanceCtrlAssetsCategory', ModalInstanceCtrlAssetsCategory);

    ModalInstanceCtrlAssetsCategory.$inject = ["$scope", '$http', 'ngAuthSettings', "CustomerCategoryService", "$modalInstance", "customercategory", 'FileUploader'];

    function ModalInstanceCtrlAssetsCategory($scope, $http, ngAuthSettings, CustomerCategoryService, $modalInstance, customercategory, FileUploader) {
        console.log("customercategory");

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





        $scope.CustomerCategoryData = {

        };
        if (customercategory) {
            console.log("CustomerCategory if conditon");

            $scope.CustomerCategoryData = customercategory;

            console.log($scope.CustomerCategoryData.CustomerCategoryName);
            //console.log($scope.ProjectData.Description);
        }


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddCustomerCategory = function (data) {


            console.log("CustomerCategory");

            var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            console.log(FileUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.CustomerCategoryData.FileUrl = FileUrl;
            console.log($scope.CustomerCategoryData.FileUrl);




            var url = serviceBase + "api/CustomerCategorys";
            var dataToPost = { CustomerCategoryName: $scope.CustomerCategoryData.CustomerCategoryName, CreatedDate: $scope.CustomerCategoryData.CreatedDate, UpdatedDate: $scope.CustomerCategoryData.UpdatedDate, CreatedBy: $scope.CustomerCategoryData.CreatedBy, UpdateBy: $scope.CustomerCategoryData.UpdateBy };
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



        $scope.PutCustomerCategory = function (data) {
            $scope.CustomerCategoryData = {

            };
            if (customercategory) {
                $scope.CustomerCategoryData = customercategory;
                console.log("found Puttt Customer Category");
                console.log(customercategory);
                console.log($scope.CustomerCategoryData);
                //console.log($scope.Customer.name);
                //console.log($scope.Customer.description);
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update Customer Category");

            var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);

            var url = serviceBase + "api/CustomerCategorys";
            var dataToPost = { CustomerCategoryId: $scope.CustomerCategoryData.CustomerCategoryId, CustomerCategoryName: $scope.CustomerCategoryData.CustomerCategoryName, CreatedDate: $scope.CustomerCategoryData.CreatedDate, UpdatedDate: $scope.CustomerCategoryData.UpdatedDate, CreatedBy: $scope.CustomerCategoryData.CreatedBy, UpdateBy: $scope.CustomerCategoryData.UpdateBy };
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
        .controller('ModalInstanceCtrldeleteAssetsCategory', ModalInstanceCtrldeleteAssetsCategory);

    ModalInstanceCtrldeleteAssetsCategory.$inject = ["$scope", '$http', "$modalInstance", "CustomerCategoryService", 'ngAuthSettings', "customercategory"];

    function ModalInstanceCtrldeleteAssetsCategory($scope, $http, $modalInstance, CustomerCategoryService, ngAuthSettings, customercategory) {
        console.log("delete modal opened");


        $scope.customercategorys = [];

        if (customercategory) {
            $scope.CustomerCategoryData = customercategory;

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deletecustomercategorys = function (dataToPost, $index) {

            CustomerCategoryService.deletecustomercategorys(dataToPost).then(function (results) {

                $scope.customercategorys.splice($index, 1);
                $scope.customercategorys.splice($index, 1);
                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();
