

(function () {
    'use strict';

    angular
        .module('app')
        .controller('profilesController', profilesController);

    profilesController.$inject = ['$scope', 'profilesService', '$http', '$location', 'ngAuthSettings', 'FileUploader', 'logger'];

    function profilesController($scope, profilesService, $http, $location, ngAuthSettings, FileUploader, logger) {
        console.log(" profile controller start loading");
        $scope.currentPageStores = {};
        $scope.peoples = [];
        //$scope.profiles = [];
        $scope.notify = function (type) {
            switch (type) {
                case "info":
                    return logger.log("Heads up! This alert needs your attention, but it's not super important.");
                case "success":
                    return logger.logSuccess("Well done! You successfully updated your Profile.");
                case "warning":
                    return logger.logWarning("Warning! Best check yo self, you're not looking too good.");
                case "error":
                    return logger.logError("Oh snap! Change a few things up and try submitting again.")
            }
        }

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

        $scope.peoples = [];
        $scope.currentPageStores = {};
        profilesService.getpeoples().then(function (results) {
            console.log("Get Method Called");
            $scope.peoples = results.data;
            console.log($scope.peoples);
            //$scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });
        console.log("abc");
        
        $scope.userRole = null;
        $scope.localData = JSON.parse(localStorage.getItem('RolePerson'));
        // var role = roles ? roles.filter(x=> x == 'HQ Master login' || x =='WH Master login') : null;
        //if(role[0] == 'HQ Master login' || role[0] == 'WH Master login'){
        if (parseInt($scope.localData.Warehouseid) == 0) {
            $scope.userRole = $scope.localData.Warehouseid;
        } else {
            $scope.userRole = null;
        }
        if ($scope.localData.Warehouseids) {
            $scope.warehouses = $scope.localData.Warehouseids.split(',').map(Number);
        }
        //$scope.userName = null;
        //$scope.UserRoles = JSON.parse(localStorage.getItem('RoleNames'));
        //var roles = $scope.UserRoles.split(',');
        //var isAdminRole = roles ? roles.filter(x => x == 'HQ Master login') : null;      
        //if (isAdminRole[0] == 'HQ Master login') {
        //    $scope.dontShowButton = false;
        //}
        $scope.warehouseSelection = function () {
            
            var preURI = saralUIPortal + "token";
            var uri = "layout/warehouse-selection"
            var redirectURL = uri.replace(/\//g, '---');  //'/layout---Account---ladgerreport/';
            //var token = JSON.stringify(jwtToken);
            var token = $scope.localData.access_token;
            var Warehouseids = $scope.localData.Warehouseids;
            var userid = $scope.localData.userid;
            $scope.userName = $scope.localData.userName;
            window.location.replace(preURI + "/" + redirectURL + "/" + token + "/" + Warehouseids + "/" + userid + "/" + $scope.userName);
        }




        $scope.PutProfile = function (data) {
            
            console.log("insavefn");
            $scope.profiles = [];

            //var ImageUrl = "../../UploadedImages/" + $scope.uploadedfileName;
            var ImageUrl = serviceBase + "../../UploadedImages/" + $scope.uploadedfileName;
            //var ImageUrl = serviceBase + $scope.uploadedfileName;

            console.log(ImageUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.peoples.ImageUrl = ImageUrl;
            console.log($scope.peoples.ImageUrl);

            ////$scope.peoples = results.data;
            console.log($scope.peoples);
            var url = serviceBase + "/api/Profiles/UpdatePeopleProfile";
            var dataToPost = { PeopleID: data.PeopleID, CompanyId: data.CompanyId, PeopleFirstName: data.PeopleFirstName, PeopleLastName: data.PeopleLastName, Email: data.Email, CreatedDate: data.CreatedDate, UpdatedDate: data.UpdatedDate, CreatedBy: data.CreatedBy, UpdateBy: data.UpdateBy, Department: data.Department, BillableRate: data.BillableRate, Permissions: data.Permissions, ImageUrl: data.ImageUrl, Type: data.Type, CostRate: data.CostRate, Mobile: data.Mobile }
            console.log(data.PeopleID);
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {
                    //console.log("Error Gor Here");
                    //console.log(data);
                    //$scope.notify('success');
                    //$location.path('/pages/profile');
                    //if (data.id == 0) {
                    //    $scope.gotErrors = true;
                    //    if (data[0].exception == "Already") {
                    //        console.log("Got This User Already Exist");
                    //        $scope.AlreadyExist = true;
                    //    }
                    //}
                    if (data.status) {
                        logger.logSuccess(data.Message);
                    }
                    else {
                        return logger.logError(data.Message)
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
            url: serviceBase + 'api/imageupload'
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


