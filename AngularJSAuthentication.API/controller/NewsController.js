

(function () {
    'use strict';

    angular
        .module('app')
        .controller('NewsController', NewsController);

    NewsController.$inject = ['$scope', '$http', '$modal', "$filter", "NewsService"];

    function NewsController($scope, $http, $modal, $filter, NewsService) {

        $scope.currentPageStores = {};
        $scope.News = [];
        NewsService.getNews().then(function (results) {
            $scope.News = results.data;
            console.log($scope.News);
            $scope.callmethod();
        });

        $scope.open = function () {
            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "NewsADDController", resolve: { object: function () { return $scope.News } }
                });
            modalInstance.result.then(function (selectedNews) {
                    $scope.currentPageStores.push(selectedNews);


                },
                    function () {

                    })
        };

        $scope.edit = function (News) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myputmodal.html",
                    controller: "NewsADDController", resolve: { object: function () { return News } }
                });
            modalInstance.result.then(function (selectedNews) {
                    $scope.NewsDb.push(selectedNews);
                    _.find($scope.NewsDb, function (NewsDb) {
                        if (NewsDb.id == selectedNews.id) {
                            NewsDb = selectedNews;
                        }
                    });
                    $scope.NewsDb = _.sortBy($scope.NewsDb, 'Id').reverse();
                    $scope.selected = selectedNews;

                },
                    function () {

                    })
        };

        $scope.opendelete = function (data, $index) {
            console.log(data);
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mydeletemodal.html",
                    controller: "NewsdeleteController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedComplain) {
                    if (selectedComplain == null) {

                    } else { $scope.currentPageStores.splice($index, 1); }
                },
                    function () {
                    })
        };


        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.News;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [3, 5, 10, 20, 50];
            $scope.numPerPage = $scope.numPerPageOpt[3];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select($scope.currentPage);
        }
        $scope.select = function (page) {
            var end, start;
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            $scope.select(1); $scope.currentPage = 1, $scope.row = "";
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
        .controller('NewsADDController', NewsADDController);

    NewsADDController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "NewsService", "FileUploader", "unitMasterService"];

    function NewsADDController($scope, $http, ngAuthSettings, $modalInstance, object, ItemService, FileUploader, unitMasterService) {

        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();

        $scope.saveData = {};

        if (object) {
            $scope.saveData = object;
        }

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

        $scope.Newsunits = [];
        unitMasterService.getunitMaster().then(function (results) {
            $scope.Newsunits = results.data;
        })

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.Add = function (data) {

                console.log(data);
                var LogoUrl = $scope.uploadedfileName;
                $scope.saveData.Image = serviceBase + "../../UploadedLogos/" + LogoUrl;

                var url = serviceBase + "api/NewsApi";

                var dataToPost = {
                    NewsName: $scope.saveData.NewsName,
                    Description: $scope.saveData.Description,
                    Image: $scope.saveData.Image,
                    IsAvailable: $scope.saveData.IsAvailable,
                };

                $http.post(url, dataToPost)
                    .success(function (data) {
                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {

                            $modalInstance.close(data);
                        }
                    })
                    .error(function (data) {

                    })
            };

        $scope.Put = function (object) {

            $scope.saveData = {

            };
            if (object) {
                $scope.saveData = object;
            }
            $scope.logourl = object.Image;


            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {
                $scope.ImgToUpload = $scope.logourl;
            }
            else {
                var LogoUrl = $scope.uploadedfileName;
                $scope.saveData.Image = serviceBase + "../../UploadedLogos/" + LogoUrl;
                $scope.ImgToUpload = serviceBase + "../../UploadedLogos/" + $scope.saveData.Image;
            }

            var dataToPost = {
                NewsId: $scope.saveData.NewsId,
                NewsName: $scope.saveData.NewsName,
                Description: $scope.saveData.Description,
                Image: $scope.saveData.Image,
                IsDeleted: $scope.saveData.IsDeleted,
                IsAvailable: $scope.saveData.IsAvailable,

            };


            var url = serviceBase + "api/NewsApi";
            $http.put(url, dataToPost)
                .success(function (data) {
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {

                })

        };

        /////////////////////////////////////////////////////// angular upload cod

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
        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploader.onAfterAddingFile = function (fileItem) {
        };
        uploader.onAfterAddingAll = function (addedFileItems) {
        };
        uploader.onBeforeUploadItem = function (item) {
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
            alert("Image Uploaded Successfully");
        };
        uploader.onCompleteAll = function () {
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('NewsdeleteController', NewsdeleteController);

    NewsdeleteController.$inject = ["$scope", '$http', "$modalInstance", "NewsService", 'ngAuthSettings', "object"];

    function NewsdeleteController($scope, $http, $modalInstance, NewsService, ngAuthSettings, object) {

        $scope.News = [];
        if (object) {
            $scope.saveData = object;

        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.delete = function (dataToPost, $index) {
            NewsService.deleteNews(dataToPost).then(function (results) {
                if (results.data == '"error"') {
                    alert("News Cannot Be Deleted As It Is Associated With Some Category!");
                    $modalInstance.close(null);
                    return false;
                } else if (results.data == '"success"') {
                    alert("News Deleted Successfully!");
                    $modalInstance.close(dataToPost);
                }
            });
        }

    }
})();