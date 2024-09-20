

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CategoryImageController', CategoryImageController);

    CategoryImageController.$inject = ['$scope', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function CategoryImageController($scope, CategoryService, $filter, $http, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            console.log(" category Controller reached");
            $scope.currentPageStores = {};

            $scope.open = function () {

                console.log("Modal opened ");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myCategoryModal.html",
                        controller: "ModalInstanceCtrlCategoryAddimages", resolve: { category: function () { return $scope.items } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            $scope.edit = function (item) {

                console.log("Edit Dialog called survey");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myCategoryModalPut.html",
                        controller: "ModalInstanceCtrlCategoryEditimages", resolve: { category: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.categorys.push(selectedItem);
                    _.find($scope.categorys, function (category) {
                        if (category.id == selectedItem.id) {
                            category = selectedItem;
                        }
                    });
                    $scope.categorys = _.sortBy($scope.categorys, 'Id').reverse();
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
                        controller: "ModalInstanceCtrlCategoryeditimage", resolve: { category: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.categorys.push(selectedItem);
                    _.find($scope.categorys, function (category) {
                        if (category.id == selectedItem.id) {
                            category = selectedItem;
                        }
                    });
                    $scope.categorys = _.sortBy($scope.categorys, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
            };

            $scope.opendelete = function (data, $index) {
                console.log(data);
                console.log("Delete Dialog called for category");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModaldeleteCategory.html",
                        controller: "ModalInstanceCtrldeleteCategory", resolve: { category: function () { return data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            $scope.categoryImages = [];
            var url = serviceBase + "api/CategoryImage/GetCategoryImage";
            $http.get(url)
                .success(function (results) {

                    $scope.categoryImages = results;
                    $scope.callmethod();

                });
            $scope.callmethod = function () {
                var init;
                $scope.stores = $scope.categoryImages;
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
        
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlCategoryAddimages', ModalInstanceCtrlCategoryAddimages);

    ModalInstanceCtrlCategoryAddimages.$inject = ["$scope", '$http', 'ngAuthSettings', "CategoryService", "$modalInstance", "category", 'FileUploader'];

    function ModalInstanceCtrlCategoryAddimages($scope, $http, ngAuthSettings, CategoryService, $modalInstance, category, FileUploader) {
        console.log("category");
        var input = document.getElementById("file");

        var today = new Date();
        $scope.today = today.toISOString();

        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });

        ////for image
        $scope.uploadedfileName = '';
        $scope.upload = function (files) {
            console.log(files);
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    console.log(config.file.name);

                    console.log("File Name is " + $scope.uploadedfileName);
                    //var fileuploadurl = '/api/logoUpload/UploadCategoryImage', files;
                    var fileuploadurl = '/api/logoUpload/UploadCategoryImage';
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

        $scope.Categories = [];
        var caturl = serviceBase + "api/CategoryImage";
        $http.get(caturl)
            .success(function (data) {
                console.log("Got Here");
                console.log(data);
                $scope.Categories = data;
            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            })

        $scope.CategoryData = {};
        if (category) {
            console.log("category if conditon");
            $scope.CategoryData = category;
            console.log($scope.CategoryData.CategoryName);
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };



        $scope.AddCategory = function (data) {
            var LogoUrl = $scope.uploadedfileName;
            if (LogoUrl == "") {
                alert("Please upload  image before saving");
                return;
            }
            console.log("Add category");
            var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
            console.log(LogoUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.CategoryData.LogoUrl = $scope.LogoUrl;
            console.log($scope.CategoryData.LogoUrl);
            var url = serviceBase + "api/CategoryImage";
            var dataToPost = {
                CategoryImg: $scope.CategoryData.LogoUrl,
                Categoryid: data.Categoryid,
                IsActive: true,
                LogoUrl: $scope.CategoryData.LogoUrl,
                AppType: $scope.CategoryData.AppType
            };
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (data) {

                    console.log("Error Gor Here");
                    console.log(data);
                    if (data == "1") {
                        alert("Category Image Added");
                        $modalInstance.close(data);

                        window.location.reload("true");
                    }
                    else {
                        alert("Not Added Some Error");
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };

        $scope.PutCategory = function (data) {
            $scope.CategoryData = {};
            $scope.loogourl = data.CategoryImg;

            if (category) {
                $scope.CategoryData = category;
                console.log("found Puttt category");
                console.log(category);
                console.log($scope.CategoryData);
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update category");
            if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {

                console.log("if looppppppppppp");
                var url = serviceBase + "api/CategoryImage";
                var dataToPost = {
                    CategoryImageId: $scope.CategoryData.CategoryImageId,
                    CategoryImg: $scope.loogourl,
                    CategoryId: $scope.CategoryData.CategoryId,
                    IsActive: $scope.CategoryData.IsActive,
                    CreatedDate: $scope.CategoryData.CreatedDate,
                    AppType: $scope.CategoryData.AppType
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
                        alert(" CategoryImage updated Succefully");
                        $modalInstance.close(data);
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }
            else {

                console.log("Else loppppppppp ");
                var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
                console.log(LogoUrl);
                console.log("Image name in Insert function :" + $scope.uploadedfileName);
                $scope.CategoryData.LogoUrl = LogoUrl;
                console.log($scope.CategoryData.LogoUrl);
                var url2 = serviceBase + "api/CategoryImage";//instead of url used url2
                var dataToPost2 = { //instead of dataToPost Used dataToPost2
                    CategoryImageId: $scope.CategoryData.CategoryImageId,
                    CategoryImg: $scope.LogoUrl,
                    CategoryId: $scope.CategoryData.CategoryId,
                    IsActive: $scope.CategoryData.IsActive,
                    CreatedDate: $scope.CategoryData.CreatedDate
                };
                console.log(dataToPost2);
                $http.put(url2, dataToPost2)
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
                    })
            }
        };
        /////////////////////////////////////////////////////// angular upload code
        $scope.sa = $scope.CategoryData.CategoryImageId
        // $scope.sa = category.CategoryImageId;
        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/logoUpload'
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
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            var fileName = fileItem.file.name.split(".")[0];
            fileItem.file.name = fileName;
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
            response = response.slice(1, -1);
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
        .controller('ModalInstanceCtrlCategoryEditimages', ModalInstanceCtrlCategoryEditimages);

    ModalInstanceCtrlCategoryEditimages.$inject = ["$scope", '$http', 'ngAuthSettings', "CategoryService", "$modalInstance", "category", 'FileUploader'];

    function ModalInstanceCtrlCategoryEditimages($scope, $http, ngAuthSettings, CategoryService, $modalInstance, category, FileUploader) {

        console.log("category");
        var input = document.getElementById("file");

        var today = new Date();
        $scope.today = today.toISOString();

        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });

        ////for image
        $scope.uploadedfileName = '';
        $scope.upload = function (files) {
            console.log(files);
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    console.log(config.file.name);

                    console.log("File Name is " + $scope.uploadedfileName);
                    //var fileuploadurl = '/api/logoUpload/UploadCategoryImage', files;
                    var fileuploadurl = '/api/logoUpload/UploadCategoryImage';
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

        $scope.Categories = [];
        var caturl = serviceBase + "api/CategoryImage";
        $http.get(caturl)
            .success(function (data) {
                console.log("Got Here");
                console.log(data);
                $scope.Categories = data;
            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            })

        $scope.CategoryData = {};
        if (category) {
            console.log("category if conditon");
            $scope.CategoryData = category;
            console.log($scope.CategoryData.CategoryName);
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.PutCategory = function (data) {

            $scope.CategoryData = {};
            $scope.loogourl = data.CategoryImg;

            if (category) {
                $scope.CategoryData = category;
                console.log("found Puttt category");
                console.log(category);
                console.log($scope.CategoryData);
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update category");
            if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {

                console.log("if looppppppppppp");
                var url = serviceBase + "api/CategoryImage";
                var dataToPost = {
                    CategoryImageId: $scope.CategoryData.CategoryImageId,
                    CategoryImg: $scope.loogourl,
                    CategoryId: $scope.CategoryData.CategoryId,
                    IsActive: $scope.CategoryData.IsActive,
                    CreatedDate: $scope.CategoryData.CreatedDate,
                    AppType: $scope.CategoryData.AppType,
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
                        alert(" CategoryImage updated Succefully");
                        $modalInstance.close(data);
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }
            else {
                
                console.log("Else loppppppppp ");
                var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
                console.log(LogoUrl);
                console.log("Image name in Insert function :" + $scope.uploadedfileName);
                $scope.CategoryData.LogoUrl = $scope.LogoUrl;
                console.log($scope.CategoryData.LogoUrl);
                var url1 = serviceBase + "api/CategoryImage";//instead of url used url1
                var dataToPost1 = {//instead of dataToPost used dataToPost1
                    CategoryImageId: $scope.CategoryData.CategoryImageId,
                    CategoryImg: $scope.CategoryData.LogoUrl,
                    CategoryId: $scope.CategoryData.CategoryId,
                    IsActive: $scope.CategoryData.IsActive,
                    CreatedDate: $scope.CategoryData.CreatedDate,
                    LogoUrl: $scope.LogoUrl
                };
                console.log(dataToPost1);
                $http.put(url1, dataToPost1)
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
                    })
            }
        };

        /////////////////////////////////////////////////////// angular upload code
        $scope.sa = category.CategoryImageId;//381
        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/logoUpload'
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
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            var fileName = fileItem.file.name.split(".")[0];
            fileItem.file.name = fileName;
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
            response = response.slice(1, -1); //For remove 
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
        .controller('ModalInstanceCtrldeleteCategory', ModalInstanceCtrldeleteCategory);

    ModalInstanceCtrldeleteCategory.$inject = ["$scope", '$http', "$modalInstance", "CategoryService", 'ngAuthSettings', "category"];

    function ModalInstanceCtrldeleteCategory($scope, $http, $modalInstance, CategoryService, ngAuthSettings, category) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        }
        $scope.categorys = [];
        if (category) {
            $scope.CategoryData = category;
            console.log("found category");
            console.log(category.Categoryid);
            console.log($scope.CategoryData);
        }
        $scope.ok = function () {

            $modalInstance.close();
        };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.deleteCategory = function (dataToPost, $index) {
            console.log("Delete  category controller");
            $http.delete(serviceBase + 'api/CategoryImage/?id=' + $scope.CategoryData.CategoryImageId).then(function (results) {
                console.log(results);
                console.log("Del");
                console.log(results);
                console.log("index of item " + $index);
                console.log($scope.categorys);
                $modalInstance.close(dataToPost);
                return results;
                //console.log("Del");
                //console.log(results);
                //console.log("index of item " + $index);
                //console.log($scope.categorys);
                //$modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });
            $modalInstance.close();
        }
    }
})();