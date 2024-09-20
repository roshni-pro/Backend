

(function () {
    'use strict';

    angular
        .module('app')
        .controller('SliderCtrl', SliderCtrl);

    SliderCtrl.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', "SliderService"];

    function SliderCtrl($scope, $filter, $http, ngTableParams, $modal, SliderService) {

        $scope.currentPageStores = {};
        $scope.Sliders = [];

        $scope.open = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "SliderADDCtrl", resolve: { object: function () { return $scope.items } }
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
                    controller: "SliderADDCtrl", resolve: { object: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.Sliders.push(selectedItem);
                    _.find($scope.Sliders, function (Sliders) {
                        if (Sliders.id == selectedItem.id) {
                            Sliders = selectedItem;
                        }
                    });
                    $scope.Sliders = _.sortBy($scope.Sliders, 'Id').reverse();
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
                    controller: "Sliderdeletectrl", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                    SliderService.getSliders().then(function (results) {
                        $scope.Sliders = results.data;
                        $scope.callmethod();
                    })

                },
                    function () {
                    })

        };

        SliderService.getSliders().then(function (results) {
            $scope.Sliders = results.data;
            $scope.callmethod();
        })


        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.Sliders;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

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

            $scope.numPerPageOpt = [3, 5, 10, 20, 50];
            $scope.numPerPage = $scope.numPerPageOpt[3];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('SliderADDCtrl', SliderADDCtrl);

    SliderADDCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "SliderService", "FileUploader"];

    function SliderADDCtrl($scope, $http, ngAuthSettings, $modalInstance, object, SliderService, FileUploader) {

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
                    //var fileuploadurl = '/api/SliderUpload/post', files;
                    var fileuploadurl = '/api/SliderUpload/post';
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


        $scope.saveData = {};

        if (object) {
            $scope.saveData = object;
        }

        $scope.Sliders = [];

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.Add = function (data) {

                var LogoUrl = $scope.uploadedfileName;

                $scope.saveData.Type = "Slider";
                if (LogoUrl == "") {
                    alert("Please upload slider image before saving");
                    return;
                }

                $scope.saveData.Pic = LogoUrl;
                var url = serviceBase + "api/Slider";
                var dataToPost = { Type: $scope.saveData.Type, isWeb: $scope.saveData.isWeb, SequianceNumber: $scope.saveData.SequianceNumber, Pic: $scope.saveData.Pic };
                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {
                        $modalInstance.close(data);
                    })
                    .error(function (data) {
                    })
            };


        $scope.Put = function (data) {
            $scope.saveData = {};
            if (object) {
                $scope.saveData = object;
            }
            $scope.loogourl = object.Pic;



            $scope.saveData.Type = "Slider";
            if ($scope.loogourl == "") {
                alert("Please upload slider image before saving");
                return;
            }

            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


            if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {
                var url = serviceBase + "api/Slider";
                var dataToPost = { SliderId: $scope.saveData.SliderId, isWeb: $scope.saveData.isWeb, SequianceNumber: $scope.saveData.SequianceNumber, Type: $scope.saveData.Type, Pic: $scope.loogourl, CreatedDate: $scope.saveData.CreatedDate, UpdatedDate: $scope.saveData.UpdatedDate };
                $http.put(url, dataToPost)
                    .success(function (data) {
                        $modalInstance.close(data);
                    })
                    .error(function (data) {

                    })
            }
            else {
                var LogoUrl = $scope.uploadedfileName;
                $scope.saveData.Pic = LogoUrl;
                var url1 = serviceBase + "api/Slider";
                var dataToPost1 = { SliderId: $scope.saveData.SliderId, Type: $scope.saveData.Type, Pic: $scope.saveData.Pic, CreatedDate: $scope.saveData.CreatedDate, UpdatedDate: $scope.saveData.UpdatedDate };
                $http.put(url1, dataToPost1)
                    .success(function (data) {
                        $modalInstance.close(data);
                    })
                    .error(function (data) {

                    })
            }

        };

        /////////////////////////////////////////////////////// angular upload code


        var uploader = $scope.uploader = new FileUploader({
            url: 'api/SliderUpload'
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
        .controller('Sliderdeletectrl', Sliderdeletectrl);

    Sliderdeletectrl.$inject = ["$scope", '$http', "$modalInstance", "SliderService", 'ngAuthSettings', "object"];

    function Sliderdeletectrl($scope, $http, $modalInstance, SliderService, ngAuthSettings, object) {

        $scope.group = [];
        if (object) {
            $scope.saveData = object;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.delete = function (dataToPost, $index) {
            SliderService.deleteSliders(dataToPost).then(function (results) {
                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });
        }


    }
})();