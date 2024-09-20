

(function () {
    'use strict';

    angular
        .module('app')
        .controller('QuestionController', QuestionController);

    QuestionController.$inject = ['$scope', 'CustomerCategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function QuestionController($scope, CustomerCategoryService, $filter, $http, ngTableParams, $modal) {

        $scope.items = {};

        $scope.AddQuestion = function () {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myQuestionModal.html",
                    controller: "AddQuestionController", resolve: { customercategory: function () { return $scope.items; } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };

        $scope.EditQuestion = function (trade) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myEditQuestionModal.html",
                    controller: "AddQuestionController", resolve: { customercategory: function () { return trade } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                });
        };

        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for Game");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteGameQuestion.html",
                    controller: "AddQuestionController", resolve: { customercategory: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        function GetQuestion() {
            var url = serviceBase + "api/Game/GetQuestion";
            $http.get(url).success(function (response) {

                $scope.Question = response;
                $scope.callmethod();
                //localStorage.removeItem('sample_data');
                console.log($scope.SupplierPaymentData);
            })
                .error(function (data) {
                });
        }
        GetQuestion();


        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.Question;

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
        };

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        };

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        };

        $scope.search = function () {
            console.log("search");
            console.log($scope.stores);
            console.log($scope.searchKeywords);

            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        };

        $scope.order = function (rowName) {
            console.log("order"); console.log($scope.stores);
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        };
                      

        ////end
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AddQuestionController', AddQuestionController);

    AddQuestionController.$inject = ["$scope", '$http', 'ngAuthSettings', "CustomerCategoryService", "$modalInstance", "customercategory", 'FileUploader'];

    function AddQuestionController($scope, $http, ngAuthSettings, CustomerCategoryService, $modalInstance, customercategory, FileUploader) {
        console.log("customercategory");

        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();

        $scope.Questiondata = customercategory;

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.Question = {};

        $scope.AnswerOptions = [
            { id: "Yes", name: 'Yes' },
            { id: "No", name: 'No' }
        ];

        function GetGameLevel() {

            var url = serviceBase + "api/Game/GetGameLevel";
            $http.get(url).success(function (response) {
                $scope.Levels = response;
                //localStorage.removeItem('sample_data');
                console.log($scope.SupplierPaymentData);
            })
                .error(function (data) {
                })
        }
        GetGameLevel();

        ////for image
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
        $scope.AddQuestion = function (Question) {

            if (Question.GameLevelId === undefined || Question.GameLevelId === "") {
                alert("Please insert Game Level");
                return false;
            }
            if (Question.GameQuestionData === undefined || Question.GameQuestionData === "") {
                alert("Please insert Question");
                return false;
            }
            if (Question.GameAnswer === undefined || Question.GameAnswer === "") {
                alert("Please insert Answer");
                return false;
            }
            if (Question.QuestionPoints === undefined || Question.QuestionPoints === "") {
                alert("Please insert Question Points");
                return false;
            }

            else {

                var urldata = serviceBase + "api/Game/GetGameLevelCount?GameLevelid=" + Question.GameLevelId;
                $http.get(urldata).success(function (response) {

                    $scope.QuestionCount = response.count;
                    if ($scope.QuestionCount > 10) {
                        alert("Already Added 10 Question to this level");
                    }
                    else {
                        var url = serviceBase + "api/Game/AddQuestion";
                        var dataToPost = {
                            GameQuestionData: Question.GameQuestionData,
                            GameQuestionHindi: Question.GameQuestionHindi,
                            GameAnswer: Question.GameAnswer,
                            QuestionPoints: Question.QuestionPoints,
                            CreatedBy: $scope.UserRole.userid,
                            UpdateBy: $scope.UserRole.userid,
                            WarehouseId: $scope.UserRole.Warehouseid,
                            IsActive: true,
                            GameLevelId: Question.GameLevelId,
                            GameImage: $scope.uploadedfileName
                        };
                        $http.post(url, dataToPost)
                            .success(function (data) {
                                alert('Question Added Successfully');
                                $modalInstance.close();
                                window.location.reload();
                            })
                            .error(function (data) {
                                console.log("Error Got Heere is ");
                                console.log(data);
                            });
                    }
                });
            }

        };

        $scope.deletegameQuestion = function () {

            var url = serviceBase + "api/Game/DeleteGameQuestion?GameQuestionId=" + $scope.Questiondata.GameQuestionId;
            $http.delete(url)
                .success(function (data) {
                    console.log(data);
                    window.location.reload();
                }
                ).error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                });

        };

        $scope.EditQuestion = function (Question) {

            if (Question.GameLevelId === undefined || Question.GameLevelId === "") {
                alert("Please insert Game Level");
                return false;
            }
            if (Question.GameQuestionData === undefined || Question.GameQuestionData === "") {
                alert("Please insert Question");
                return false;
            }
            if (Question.GameAnswer === undefined || Question.GameAnswer === "") {
                alert("Please insert Answer");
                return false;
            }
            if (Question.QuestionPoints === undefined || Question.QuestionPoints === "") {
                alert("Please insert Question Points");
                return false;
            }
            if ($scope.uploadedfileName === "") {
                $scope.uploadedfileName = null;
            }
            else {

                var urldata = serviceBase + "api/Game/GetGameLevelCount?GameLevelid=" + Question.GameLevelId;
                $http.get(urldata).success(function (response) {
                    $scope.QuestionCount = response.count;
                    if ($scope.QuestionCount > 10) {
                        alert("Already Added 10 Question to this level");
                    }
                    else {
                        var url = serviceBase + "api/Game/EditQuestion";
                        var dataToPost = {
                            GameQuestionId: Question.GameQuestionId,
                            GameQuestionData: Question.GameQuestionData,
                            GameQuestionHindi: Question.GameQuestionHindi,
                            GameAnswer: Question.GameAnswer,
                            QuestionPoints: Question.QuestionPoints,
                            CreatedBy: $scope.UserRole.userid,
                            UpdateBy: $scope.UserRole.userid,
                            WarehouseId: $scope.UserRole.Warehouseid,
                            IsActive: true,
                            GameLevelId: Question.GameLevelId,
                            GameImage: $scope.uploadedfileName
                        };
                        $http.post(url, dataToPost)
                            .success(function (data) {
                                alert('Question Updated Successfully');
                                $modalInstance.close();
                                window.location.reload();
                            })
                            .error(function (data) {
                                console.log("Error Got Heere is ");
                                console.log(data);
                            });
                    }

                });
            }

        };

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        var uploader = $scope.uploader = new FileUploader({


            url: 'api/logoUpload/UploadQuestionImage'
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

            var fileName = fileItem.file.name.split(".")[0];
            fileItem.file.name = fileName;
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

            response = response.slice(1, -1)
            $scope.uploadedfileName = response;
            alert("Image Uploaded Successfully");
        };
        uploader.onCompleteAll = function () {
        };

    }
})();