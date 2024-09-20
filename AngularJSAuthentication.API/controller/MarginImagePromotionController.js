
(function () {
    'use strict';

    angular
        .module('app')
        .controller('MarginImagePromotionController', MarginImagePromotionController);

    MarginImagePromotionController.$inject = ['$scope', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function MarginImagePromotionController($scope, CategoryService, $filter, $http, ngTableParams, $modal) {
        var User = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.wid = User.Warehouseid;
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {

            $scope.currentPageStores = {};
            $scope.open = function () {
                console.log("Modal opened ");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "MarginImageModal.html",
                        controller: "MarginImageCtrl", resolve: { category: function () { return $scope.items } }
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
                        templateUrl: "MarginImageModalPut.html",
                        controller: "MarginImageCtrl", resolve: { category: function () { return item } }
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
            $scope.opendelete = function (data, $index) {
                console.log(data);
                console.log("Delete Dialog called for category");

                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "MarginImagedeletemodal.html",
                        controller: "MarginImagedeleteCtrl", resolve: { category: function () { return data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
            };
            $scope.categorys = [];
            $http.get(serviceBase + 'api/AppPromotion').then(function (results) {
                $scope.categorys = results.data;
                $scope.callmethod();
            }, function (error) {
            });
            $scope.callmethod = function () {
                var init;
                $scope.stores = $scope.categorys;

                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";

                    

                $scope.numPerPageOpt = [3, 5, 10, 20];
                $scope.numPerPage = $scope.numPerPageOpt[2];
                $scope.currentPage = 1;
                $scope.currentPageStores = [];
                $scope.search(), $scope.select(1);
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

            $http.get(serviceBase + 'api/AppPromotion/NewlyAddedBrands').then(function (results) {
                $scope.NewlyAddedBrands = results.data;
            }, function (error) {
            });

            $http.get(serviceBase + 'api/AppPromotion/AllTopAddedItem?warehouseid' + $scope.wid).then(function (results) {
                $scope.NewlyAddedItem = results.data;
            }, function (error) {
            });
        }
        

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('MarginImageCtrl', MarginImageCtrl);

    MarginImageCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "category", 'FileUploader'];

    function MarginImageCtrl($scope, $http, ngAuthSettings, $modalInstance, category, FileUploader) {
        console.log("AppPromotion");

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
                    //var fileuploadurl = '/api/logoUpload/post', files;
                    var fileuploadurl = '/api/logoUpload/post';
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
        $scope.CategoryData = {

        };
        if (category) {
            console.log("category if conditon");
            $scope.CategoryData = category;
            console.log($scope.CategoryData.BaseCategoryName);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.AddCategory = function (data) {

            console.log("Add category");
            console.log("if looppppppppppp");
            var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
            console.log(LogoUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.CategoryData.LogoUrl = LogoUrl;
            console.log($scope.CategoryData.LogoUrl);
            var url = serviceBase + "api/AppPromotion";
            var dataToPost = {
                LogoUrl: $scope.CategoryData.LogoUrl,
                Name: $scope.CategoryData.Name,
                Discription: $scope.CategoryData.Discription,
                CreatedDate: $scope.CategoryData.CreatedDate,
                IsActive: true,
            };
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (data) {
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
                })
            // };


        };
        $scope.PutCategory = function (data) {
            $scope.CategoryData = {
            };
            $scope.loogourl = category.LogoUrl;

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
                var url = serviceBase + "api/AppPromotion";
                var dataToPost = {
                    LogoUrl: $scope.loogourl,
                    Id: $scope.CategoryData.Id,
                    Name: $scope.CategoryData.Name,
                    Discription: $scope.CategoryData.Discription,
                    IsActive: $scope.CategoryData.IsActive,
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

                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
                // };
            }
            else {
                console.log("Else loppppppppp ");
                var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
                console.log(LogoUrl);
                console.log("Image name in Insert function :" + $scope.uploadedfileName);
                $scope.CategoryData.LogoUrl = LogoUrl;
                console.log($scope.CategoryData.LogoUrl);
                var url1 = serviceBase + "api/BaseCategory";
                var dataToPost1 = {
                    LogoUrl: $scope.CategoryData.LogoUrl,
                    Id: $scope.CategoryData.Id,
                    Name: $scope.CategoryData.Name,
                    Discription: $scope.CategoryData.Discription,
                    IsActive: $scope.CategoryData.IsActive
                };
                console.log(dataToPost1);
                $http.put(url1, dataToPost1)
                    .success(function (data) {
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
                    })
            }
        };
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
        .controller('MarginImagedeleteCtrl', MarginImagedeleteCtrl);

    MarginImagedeleteCtrl.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', "category"];

    function MarginImagedeleteCtrl($scope, $http, $modalInstance, ngAuthSettings, category) {

        function ReloadPage() {
            location.reload();
        }
        $scope.categorys = [];
        if (category) {
            $scope.CategoryData = category;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.delete = function (dataToPost, $index) {
            console.log("Delete  category controller");

            $http.delete(serviceBase + 'api/AppPromotion/?id=' + dataToPost.Id).then(function (results) {
                //results;
                results();
                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });

        }

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('TotalDhamakaController', TotalDhamakaController);

    TotalDhamakaController.$inject = ['$scope', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function TotalDhamakaController($scope, CategoryService, $filter, $http, ngTableParams, $modal) {

        var User = JSON.parse(localStorage.getItem('RolePerson'));

      {

            $scope.currentPageStores = {};
            $scope.open = function () {
                console.log("Modal opened ");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "TodalDhamakaImageModal.html",
                        controller: "TotalDhamakaCtrl", resolve: { category: function () { return $scope.items } }
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
                        templateUrl: "TodayDhamakaPut.html",
                        controller: "TotalDhamakaCtrl", resolve: { category: function () { return item } }
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
            $scope.opendelete = function (data, $index) {
                console.log(data);
                console.log("Delete Dialog called for category");

                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "TodayDhamakadeletemodal.html",
                        controller: "TodayDhamakadeleteCtrl", resolve: { category: function () { return data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);

                },
                    function () {
                        console.log("Cancel Condintion");

                    })
            };
            $scope.categorys = [];
            $http.get(serviceBase + 'api/AppPromotion/GetTodayDhamaka').then(function (results) {
                $scope.categorys = results.data;
                $scope.callmethod();
            }, function (error) {
            });
            $scope.callmethod = function () {
                var init;
                return $scope.stores = $scope.categorys,

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

            $http.get(serviceBase + 'api/AppPromotion/NewlyAddedBrands').then(function (results) {
                $scope.NewlyAddedBrands = results.data;

            }, function (error) {
            });

            $scope.wid = User.Warehouseid;
            $http.get(serviceBase + 'api/AppPromotion/AllTopAddedItem?warehouseid' + $scope.wid).then(function (results) {
                $scope.NewlyAddedItem = results.data;
            }, function (error) {
            });
        }
       
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('TotalDhamakaCtrl', TotalDhamakaCtrl);

    TotalDhamakaCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "category", 'FileUploader'];

    function TotalDhamakaCtrl($scope, $http, ngAuthSettings, $modalInstance, category, FileUploader) {
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
                    //var fileuploadurl = '/api/logoUpload/post', files;
                    var fileuploadurl = '/api/logoUpload/post';
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
        $scope.CategoryData = {

        };
        if (category) {
            console.log("category if conditon");
            $scope.CategoryData = category;
            console.log($scope.CategoryData.BaseCategoryName);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.AddCategory = function (data) {

            console.log("Add category");
            console.log("if looppppppppppp");
            var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
            console.log(LogoUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.CategoryData.LogoUrl = LogoUrl;
            console.log($scope.CategoryData.LogoUrl);
            var url = serviceBase + "api/AppPromotion/AddTodayDhamaka";
            var dataToPost = {
                LogoUrl: $scope.CategoryData.LogoUrl,
                Name: $scope.CategoryData.Name,
                Discription: $scope.CategoryData.Discription,
                CreatedDate: $scope.CategoryData.CreatedDate,
                IsActive: true,
            };
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (data) {
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
                })
            // };


        };
        $scope.PutCategory = function (data) {
            $scope.CategoryData = {
            };
            $scope.loogourl = category.LogoUrl;

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
                var url = serviceBase + "api/AppPromotion/PutTodayDhamaka";
                var dataToPost = {
                    LogoUrl: $scope.loogourl,
                    Id: $scope.CategoryData.Id,
                    Name: $scope.CategoryData.Name,
                    Discription: $scope.CategoryData.Discription,
                    IsActive: $scope.CategoryData.IsActive,
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

                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
                // };
            }
            else {
                console.log("Else loppppppppp ");
                var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
                console.log(LogoUrl);
                console.log("Image name in Insert function :" + $scope.uploadedfileName);
                $scope.CategoryData.LogoUrl = LogoUrl;
                console.log($scope.CategoryData.LogoUrl);
                var url2 = serviceBase + "api/BaseCategory";
                var dataToPost2 = {
                    LogoUrl: $scope.CategoryData.LogoUrl,
                    Id: $scope.CategoryData.Id,
                    Name: $scope.CategoryData.Name,
                    Discription: $scope.CategoryData.Discription,
                    IsActive: $scope.CategoryData.IsActive
                };
                console.log(dataToPost2);
                $http.put(url2, dataToPost2)
                    .success(function (data) {
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
                    })
            }
        };
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
        .controller('TodayDhamakadeleteCtrl', TodayDhamakadeleteCtrl);

    TodayDhamakadeleteCtrl.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', "category"];

    function TodayDhamakadeleteCtrl($scope, $http, $modalInstance, ngAuthSettings, category) {
        function ReloadPage() {
            location.reload();
        }
        $scope.categorys = [];
        if (category) {
            $scope.CategoryData = category;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.delete = function (dataToPost, $index) {
            console.log("Delete  category controller");

            $http.delete(serviceBase + 'api/AppPromotion/RemoveTodayDhamaka/?id=' + dataToPost.Id).then(function (results) {
                //results;
                results();
                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });

        }
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('BulkDealController', BulkDealController);

    BulkDealController.$inject = ['$scope', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function BulkDealController($scope, CategoryService, $filter, $http, ngTableParams, $modal) {

        var User = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.wid2 = User.Warehouseid; // by YYY
        $http.get(serviceBase + 'api/AppPromotion/GetBulkItemForWeb').then(function (results) {
            $scope.NewlyAddedItem = results.data;
            $scope.checkAll = function () {

                if ($scope.selectedAll) {
                    $scope.selectedAll = false;
                } else {
                    $scope.selectedAll = true;
                }
                angular.forEach($scope.NewlyAddedItem, function (trade) {
                    trade.check = $scope.selectedAll;
                });

            };
            $scope.getselected = function (data1) {
                $scope.assignedCusts = []
                for (var i = 0; i < data1.length; i++) {
                    if (data1[i].check == true) {
                        var cs = {
                            ItemId: data1[i].ItemId,
                        }
                        $scope.assignedCusts.push(cs);
                    }
                }
                if ($scope.assignedCusts.length > 0) {
                    $http.post(serviceBase + "api/AppPromotion/SelectedItem", $scope.assignedCusts).then(function (results) {
                        alert("Added");
                        window.location.reload();
                    }, function (error) {
                        alert("Error Got Heere is ");
                    })
                } else {
                    alert("Please select checkBox");
                }
            }
        }, function (error) {
        });
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('MostSelledItemsController', MostSelledItemsController);

    MostSelledItemsController.$inject = ['$scope', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function MostSelledItemsController($scope, CategoryService, $filter, $http, ngTableParams, $modal) {
        $http.get(serviceBase + 'api/AppPromotion/MostSelledItem').then(function (results) {
            $scope.NewlyAddedItem = results.data;
        }, function (error) {
        });

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('MostSelledItemController', MostSelledItemController);

    MostSelledItemController.$inject = ['$scope', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function MostSelledItemController($scope, CategoryService, $filter, $http, ngTableParams, $modal) {
        $http.get(serviceBase + 'api/AppPromotion/MostSelledItem').then(function (results) {
            $scope.NewlyAddedItem = results.data;
        }, function (error) {
        });

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('MostSelledBrandController', MostSelledBrandController);

    MostSelledBrandController.$inject = ['$scope', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function MostSelledBrandController($scope, CategoryService, $filter, $http, ngTableParams, $modal) {
        $http.get(serviceBase + 'api/AppPromotion/MostSelledBrand').then(function (results) {
            $scope.NewlyAddedItem = results.data;
        }, function (error) {
        });

    }
})();