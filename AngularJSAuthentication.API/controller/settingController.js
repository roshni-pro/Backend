

(function () {
    'use strict';

    angular
        .module('app')
        .controller('settingsController', settingsController);

    settingsController.$inject = ['$scope', 'settingsService', '$http', '$location', 'ngAuthSettings', 'FileUploader', 'logger'];

    function settingsController($scope, settingsService, $http, $location, ngAuthSettings, FileUploader, logger) {

        var input = document.getElementById("file");
        console.log(input);

        $scope.notify = function (type) {
            switch (type) {
                case "info":
                    return logger.log("Heads up! This alert needs your attention, but it's not super important.");
                case "success":
                    return logger.logSuccess("Well done! You successfully saved Company Profile.");
                case "warning":
                    return logger.logWarning("Warning! Best check yo self, you're not looking too good.");
                case "error":
                    return logger.logError("Oh snap! Change a few things up and try submitting again.")
            }
        }

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

        console.log(" setting controller start loading");

        $scope.settings = [];
        //$scope.currentPageStores = {};
        settingsService.getsettings().then(function (results) {
            console.log("Get Method Called");
            $scope.settings = results.data;
            console.log($scope.settings);



            //$scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });
        console.log("abc");




        $scope.PutSetting = function (data) {
            console.log("insavefn");
            $scope.settings = [];


            var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
            console.log(LogoUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.settings.LogoUrl = LogoUrl;
            console.log($scope.settings.LogoUrl);

            //console.log("Update setting");
            //$scope.settings = $scope.results.data;
            //console.log($scope.settings);
            //$scope.settings=data;
            var url = serviceBase + "api/Companys";
            //console.log($scope.settings);
            //console.log("innnnn");
            //var dataToPost = { Id: $scope.TaskData.Id, Name: $scope.TaskData.Name, Discription: $scope.TaskData.Discription, CreatedDate: $scope.TaskData.CreatedDate, UpdatedDate: $scope.TaskData.UpdatedDate, CreatedBy: $scope.TaskData.CreatedBy, UpdateBy: $scope.TaskData.UpdateBy, ProjectId: $scope.TaskData.ProjectId };
            var dataToPost = { Id: data.Id, Webaddress: data.Webaddress, Address: data.Address, contactinfo: data.contactinfo, companyName: data.companyName, timezone: data.timezone, currency: data.currency, fiscalyear: data.fiscalyear, dateformat: data.dateformat, startweek: data.startweek, updated: data.updated, stayWeekOn: data.stayWeekOn, FreezeDay: data.FreezeDay, TFSUrl: data.TFSUrl, TFSUserId: data.TFSUserId, TFSPassword: data.TFSPassword, AlertDay: data.AlertDay, AlertTime: data.AlertTime, LogoUrl: $scope.settings.LogoUrl }
            console.log(data.Id);
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

                    console.log("Error Gor Here");
                    console.log(data);
                    $scope.notify('success');
                    $location.path('/settings');
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };



        ///////////////////////////////////////////////////////// angular upload code


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



