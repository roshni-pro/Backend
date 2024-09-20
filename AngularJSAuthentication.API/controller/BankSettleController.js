

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BankSettleController', BankSettleController);

    BankSettleController.$inject = ['$scope', '$http', '$timeout', '$filter', "$location", "$modal", "localStorageService", "FileUploader"];

    function BankSettleController($scope, $http, $timeout, $filter, $location, $modal, localStorageService, FileUploader) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
       {
            console.log(" BankSettleController reached");
            var input = document.getElementById("file");
            $scope.BankStockCurrencys = [];
            $scope.BanksettleAmount = function () {

                $http.get(serviceBase + 'api/CurrencyStock/BankSettleAmount').then(function (results) {

                    $scope.BankStockCurrencys = results.data;
                    console.log(" $scope.BankStockCurrencys ", $scope.BankStockCurrencys);
                    $scope.callmethod();
                });
            }
            $scope.BanksettleAmount();
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

            $scope.testk = "";
            $scope.teststatus = "";
            $scope.view = function (CurrencyBankSettleId) {


                $http.get(serviceBase + 'api/CurrencyStock/BankSettleAmountGet?id=' + CurrencyBankSettleId).then(function (results) {
                    $scope.BankStock = results.data;
                    $scope.testk = $scope.BankStock[0].DepositedBankSlip;
                    alert($scope.testk);

                    console.log(" $scope.BankStock ", $scope.BankStock);
                    $scope.teststatus = $scope.BankStock[0].status;



                });
            }

            /////////////////////////////////////////////////////// angular upload cod

            var uploader = $scope.uploader = new FileUploader({

                url: 'api/logoUpload',

            });
            //FILTERS

            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    return this.queue.length < 1;
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
            $scope.isDisabled = true;
            uploader.onCompleteItem = function (fileItem, response, status, headers) {
                $scope.uploadedfileName = fileItem._file.name;
                alert("Image Uploaded Successfully");
                $scope.isDisabled = false;

            };
            uploader.onCompleteAll = function () {

            };

            $scope.Add = function (data) {

                console.log("data", data);

                var LogoUrl = $scope.uploadedfileName;
                $scope.Image = serviceBase + "../../UploadedLogos/" + LogoUrl;


                var dataToPost = {
                    CurrencyBankSettleId: data.CurrencyBankSettleId,
                    DepositedBankSlip: $scope.Image,
                };

                console.log("dataToPost", dataToPost);
                var url = serviceBase + 'api/CurrencyStock/BankSettleAmountPut';
                $http.put(url, dataToPost)
                    .success(function (data) {
                        if (data !== null) {
                            alert("Save Success");

                            window.location.reload("true");
                        }

                    })
                    .error(function (data) {
                        alert("Not Save ");
                    })
            }
            $scope.callmethod = function () {

                var init;
                $scope.stores = $scope.BankStockCurrencys;

                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";

                $scope.numPerPageOpt = [30, 50, 100, 200];
                $scope.numPerPage = $scope.numPerPageOpt[1];
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