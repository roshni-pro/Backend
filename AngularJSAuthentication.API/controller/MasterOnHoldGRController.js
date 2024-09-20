

(function () {
    'use strict';

    angular
        .module('app')
        .controller('MasterOnHoldGRController', MasterOnHoldGRController);

    MasterOnHoldGRController.$inject = ['$scope', "$rootScope", 'WarehouseService', 'CityService', 'ngAuthSettings', '$http', '$filter', "ngTableParams", '$modal', 'FileUploader'];

    function MasterOnHoldGRController($scope, $rootScope, WarehouseService, CityService, ngAuthSettings, $http, $filter, ngTableParams, $modal, FileUploader) {

        $scope.Data = [];
        $scope.OnHoldDetails = [];

        $scope.Refresh = function () {
            location.reload();
        };
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) { });

        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;

        }, function (error) {
        });

        var uploader = $scope.uploader = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
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

            response = response.slice(1, -1);
            $scope.OnHoldGrImage = response;
            alert("Image Uploaded Successfully");
        };
        uploader.onCompleteAll = function () {
        };


        $scope.getOnHoldGR = function () {
            var url = serviceBase + 'api/OnHoldGR/getAllGR';
            $http.get(url).success(function (results) {
                $scope.OnHoldGR = results;
                $scope.callmethod();
            });
        };
        $scope.getOnHoldGR();

        $scope.OnHoldDetails = [[
            {
                'InvoiceNo': "",
                'Itemname': "",
                'MRP': "",
                'Price': "",
                'Qty': "",
                'Discription': ""
            }]];

        $scope.addNew = function () {

            $scope.OnHoldDetails.push({
                'InvoiceNo': "",
                'Itemname': "",
                'MRP': "",
                'Price': "",
                'Qty': "",
                'Discription': ""
            });
            console.log($scope.OnHoldDetails);
        };

        $scope.remove = function () {
            var newDataList = [];
            $scope.selectedAll = false;
            angular.forEach($scope.OnHoldDetails, function (selected) {
                if (!selected.selected) {
                    newDataList.push(selected);
                }
            });
            $scope.OnHoldDetails = newDataList;
        };

        $scope.ListofOnHoldGR = [];
        $scope.AddGR = function (data) {

            alert($scope.OnHoldGrImage);

            for (var i = 0; i < $scope.OnHoldDetails.length; i++) {

                var dataToPost = {
                    WarehouseId: data.WarehouseId,
                    CityId: data.CityId,
                    CityName: data.CityName,
                    InvoiceNo: data.InvoiceNo,
                    Image: $scope.OnHoldGrImage,
                    ItemName: $scope.OnHoldDetails[i].Itemname,
                    MRP: $scope.OnHoldDetails[i].MRP,
                    Price: $scope.OnHoldDetails[i].Price,
                    Qty: $scope.OnHoldDetails[i].Qty,
                    Discription: $scope.OnHoldDetails[i].Discription
                };
                $scope.ListofOnHoldGR.push(dataToPost);
                console.log(dataToPost);
            }


            console.log("getOnHoldGR");
            console.log(data);
            var url = serviceBase + "api/OnHoldGR/AddGR";


            //console.log(dataToPost);

            $http.post(url, $scope.ListofOnHoldGR)

                .success(function (data) {

                    console.log("Error Got Here");
                    console.log(data);

                    if (data.id === 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception === "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }

                    else {
                        console.log(data);
                        console.log(data);

                    }
                    confirm("SAVE DATA SUCCESSFULLY!!!");
                    window.location.reload();
                })

                .error(function (data) {
                    console.log("Error Got Here is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })


        };

        $scope.checkAll = function () {
            if (!$scope.selectedAll) {
                $scope.selectedAll = true;
            } else {
                $scope.selectedAll = false;
            }

        };



    }
})();

