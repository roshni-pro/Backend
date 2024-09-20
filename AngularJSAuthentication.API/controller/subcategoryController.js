

(function () {
    'use strict';

    angular
        .module('app')
        .controller('subcategoryController', subcategoryController);

    subcategoryController.$inject = ['$scope', 'SubCategoryService', "CategoryService", "$filter", "$http", "ngTableParams", '$modal'];

    function subcategoryController($scope, SubCategoryService, CategoryService, $filter, $http, ngTableParams, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("subcategoryController reached");

        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened sub category");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mySubCategoryModal.html",
                    controller: "ModalInstanceCtrlSubCategoryedit", resolve: { subcategory: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {


                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };




        $scope.edit = function (item) {
            console.log("Edit Dialog called subcategory");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mySubCategoryModalPut.html",
                    controller: "ModalInstanceCtrlSubCategoryedit", resolve: { subcategory: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.subcategorys.push(selectedItem);
                    _.find($scope.subcategorys, function (subcategory) {
                        if (subcategory.id == selectedItem.id) {
                            subcategory = selectedItem;
                        }
                    });

                    $scope.subcategorys = _.sortBy($scope.subcategorys, 'Id').reverse();
                    $scope.selected = selectedItem;

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };



        $scope.SetActive = function (item) {
            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "myactivemodal.html",
                    controller: "ModalInstanceCtrlSubCategoryedit", resolve: { subcategory: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.subcategorys.push(selectedItem);
                    _.find($scope.subcategorys, function (subcategory) {
                        if (subcategory.id == selectedItem.id) {
                            subcategory = selectedItem;
                        }
                    });

                    $scope.subcategorys = _.sortBy($scope.subcategorys, 'Id').reverse();
                    $scope.selected = selectedItem;

                },
                    function () {

                    })
        };


        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for subcategory");



            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteSubCategory.html",
                    controller: "ModalInstanceCtrldeletesubcategory", resolve: { subcategory: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };

        $scope.subcategorys = [];

        SubCategoryService.getsubcategorys().then(function (results) {
            console.log("ingetfn");
            console.log(results.data);
          
            $scope.subcategorys = results.data;

            $scope.callmethod();
        }, function (error) {

        });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.subcategorys;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);

        };
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
        .controller('ModalInstanceCtrlSubCategoryedit', ModalInstanceCtrlSubCategoryedit);

    ModalInstanceCtrlSubCategoryedit.$inject = ["$scope", '$http', 'ngAuthSettings', "SubCategoryService", "CategoryService", "$modalInstance", "subcategory", 'FileUploader'];

    function ModalInstanceCtrlSubCategoryedit($scope, $http, ngAuthSettings, SubCategoryService, CategoryService, $modalInstance, subcategory, FileUploader) {

        console.log("subcategory");

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
                    var fileuploadurl = '/api/upload/post', files;
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
                        cosole.log("uploaded");
                    });
                }
            }
        };



        $scope.SubCategoryData = {

        };

        $scope.categorys = [];
        CategoryService.getcategorys().then(function (results) {
            $scope.categorys = results.data;
        }, function (error) {
        });

        if (subcategory) {
            console.log("SubCategory if conditon");

            $scope.SubCategoryData = subcategory;
            console.log("kkkkkk");
            console.log($scope.SubCategoryData.Categoryid);
            console.log($scope.SubCategoryData.SubcategoryName);

        }


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };




        $scope.AddSubCategory = function (data) {
            
            var LogoUrl = $scope.uploadedfileName;
            if (LogoUrl == "") {
                alert("Please upload  image before saving");
                return;
            }
            console.log("SubCategory");



            //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            //console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);


            //var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
            var LogoUrl = $scope.LogoUrl;
            console.log(LogoUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.SubCategoryData.LogoUrl = LogoUrl;
            console.log($scope.SubCategoryData.LogoUrl);
            var url = serviceBase + "api/SubCategory";
            var dataToPost = {
                
                LogoUrl: $scope.SubCategoryData.LogoUrl,
               
                SubCategoryId: $scope.SubCategoryData.SubCategoryId,
                SubcategoryName: $scope.SubCategoryData.SubcategoryName,
                Discription: $scope.SubCategoryData.Discription,
                CreatedDate: $scope.SubCategoryData.CreatedDate,
                UpdatedDate: $scope.SubCategoryData.UpdatedDate,
                CreatedBy: $scope.SubCategoryData.CreatedBy,
                UpdateBy: $scope.SubCategoryData.UpdateBy,
                Categoryid: $scope.SubCategoryData.Categoryid,
                Code: $scope.SubCategoryData.Code,
                IsActive: true,
                HindiName: $scope.SubCategoryData.HindiName
            };
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

                        $modalInstance.close(data);
                    }
                    alert(" SubCategory Added Succefully");
                    window.location.reload();
                    $modalInstance.close(data);
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);


                })
        };



        $scope.PutSubcategory = function (data) {

            $scope.SubCategoryData = {};

            $scope.loogourl = subcategory.LogoUrl;


            if (subcategory) {
                $scope.SubCategoryData = subcategory;
                console.log("found Puttt SubCategory");
                console.log(subcategory);
                console.log($scope.SubCategoryData);

            }

            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update sub SubCategory");
            if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {

                var url = serviceBase + "api/SubCategory";
                var dataToPost = {
                    LogoUrl: $scope.loogourl,
                    SubCategoryId: $scope.SubCategoryData.SubCategoryId,
                    SubcategoryName: $scope.SubCategoryData.SubcategoryName,
                    Discription: $scope.SubCategoryData.Discription,
                    CreatedDate: $scope.SubCategoryData.CreatedDate,
                    UpdatedDate: $scope.SubCategoryData.UpdatedDate,
                    CreatedBy: $scope.SubCategoryData.CreatedBy,
                    UpdateBy: $scope.SubCategoryData.UpdateBy,
                    Categoryid: $scope.SubCategoryData.Categoryid,
                    Code: $scope.SubCategoryData.Code,
                    IsActive: $scope.SubCategoryData.IsActive,
                    HindiName: $scope.SubCategoryData.HindiName
                };


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
                        alert(" Subcategory updated Succefully");
                        $modalInstance.close(data);

                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);

                    })
            }
            else {


                var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
                console.log(LogoUrl);
                console.log("Image name in Insert function :" + $scope.uploadedfileName);
                $scope.SubCategoryData.LogoUrl = LogoUrl;
                console.log($scope.SubCategoryData.LogoUrl);
                var url = serviceBase + "api/SubCategory";
                var dataToPost = {
                    LogoUrl: $scope.LogoUrl,
                    SubCategoryId: $scope.SubCategoryData.SubCategoryId,
                    SubcategoryName: $scope.SubCategoryData.SubcategoryName,
                    Discription: $scope.SubCategoryData.Discription,
                    CreatedDate: $scope.SubCategoryData.CreatedDate,
                    UpdatedDate: $scope.SubCategoryData.UpdatedDate,
                    CreatedBy: $scope.SubCategoryData.CreatedBy,
                    UpdateBy: $scope.SubCategoryData.UpdateBy,
                    Categoryid: $scope.SubCategoryData.Categoryid,
                    Code: $scope.SubCategoryData.Code,
                    IsActive: $scope.SubCategoryData.IsActive
                };


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
                            alert(" SubCategory Update Succefully");
                            $modalInstance.close(data);
                        }
                    })


                    .error(function (data) {
                            console.log("Error Got Heere is ");
                            console.log(data);
                        })
                    }
            
        };

            
            /////////////////////////////////////////////////////// angular upload code

          

            var uploader = $scope.uploader = new FileUploader({
                url: serviceBase + 'api/logoUpload/UploadSubCategoryImage'
            });

            //FILTERS

            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    return this.queue.length < 10;
                }
            });

            //CALLBACKS
        $scope.sa = $scope.SubCategoryData.SubCategoryId;
            //$scope.sa = subcategory.SubCategoryId;//178

            uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
                console.info('onWhenAddingFileFailed', item, filter, options);
            };
            uploader.onAfterAddingFile = function (fileItem) {
                console.info('onAfterAddingFile', fileItem);
                var fileExtension = '.' + fileItem.file.name.split('.').pop();
                fileItem.file.name = $scope.sa;
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
                $scope.LogoUrl = response;
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
        .controller('ModalInstanceCtrldeletesubcategory', ModalInstanceCtrldeletesubcategory);

    ModalInstanceCtrldeletesubcategory.$inject = ["$scope", '$http', "$modalInstance", "SubCategoryService", 'ngAuthSettings', "subcategory"];

    function ModalInstanceCtrldeletesubcategory($scope, $http, $modalInstance, SubCategoryService, ngAuthSettings, subcategory) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        };



        $scope.subcategorys = [];

        if (subcategory) {
            $scope.SubCategoryData = subcategory;
            console.log("found  subcategory");
            console.log(subcategory);
            console.log($scope.SubCategoryData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deletesubCategory = function (dataToPost, $index) {

            console.log("Delete subcategorys");

            SubCategoryService.deletesubcategorys(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                console.log($scope.subcategorys.length);
                //$scope.subcategorys.splice($index, 1);
                console.log($scope.subcategorys.length);

                $modalInstance.close(dataToPost);
                //ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }
    }
})();