

(function () {
    'use strict';

    angular
        .module('app')
        .controller('NotificationController', NotificationController);

    NotificationController.$inject = ['$scope', "$filter", "$http", "ngTableParams", "FileUploader", '$modal', "customerService", "NotificationService"];

    function NotificationController($scope, $filter, $http, ngTableParams, FileUploader, $modal, customerService, NotificationService) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
       {
            $scope.currentPageStores = {};
            $scope.pageno = 1; // initialize page no to 1
            $scope.total_count = 0;
            $scope.itemsPerPage = 5; //this could be a dynamic value from a drop down
            $scope.numPerPageOpt = [5, 100, 200, 300];//dropdown options for no. of Items per page
            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;
                $scope.getNotificationdata($scope.pageno);
            };
            $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown
            $scope.$on('$viewContentLoaded', function () {
                $scope.getNotificationdata($scope.pageno);
            });
            $scope.getNotificationdata = function (pageno) {

                $scope.currentPageStores = {};
                var url = serviceBase + "api/Notification/get" + "?list=" + $scope.itemsPerPage + "&page=" + pageno;

                $http.get(url)
                    .success(function (results) {
                        $scope.currentPageStores = results.notificationmaster;
                        $scope.total_count = results.total_count;
                    })
                    .error(function (data) {
                        console.log(data);
                    })
            };
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
                        //var fileuploadurl = '/api/logoUploadNotification', files;
                        var fileuploadurl = '/api/logoUploadNotification';
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
            var uploader = $scope.uploader = new FileUploader({
                url: 'api/logoUploadNotification'
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
                fileItem.file.name = Math.random().toString(36).substring(3);
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
                response = response.slice(1, -1);
                $scope.logourl = response;
                
                alert("Image Uploaded Successfully");
            };
            uploader.onCompleteAll = function () {
            };
            $scope.datrange = '';

            $scope.datefrom = '';
            $scope.dateto = '';
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'DD/MM/YYYY'
                });
                $('.input-group-addon').click(function () {
                    $('input[name="daterange"]').trigger("select");
                    //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

                });

            });
            $scope.filtype = "";
            $scope.type = "";
            var titleText = "";
            var legendText = "";

            $scope.catType = [
                { value: 1, text: "Category" }
            ];
            $scope.selectType = [
                { value: 1, text: "Retailer" },
                { value: 2, text: "Hub" },
                { value: 3, text: "City" },
                { value: 5, text: "Cluster" }
            ];

            $scope.selectTypehub = function () {
                var url = serviceBase + "api/Warehouse";
                $http.get(url).success(function (response) {
                    $scope.selecthub = response;
                });
            }
            $scope.selectTypehub();

            $scope.selectTypecity = function () {
                var url = serviceBase + "api/City";
                $http.get(url).success(function (response) {
                    $scope.selectcity = response;
                });
            }
            $scope.selectTypecity();


            $scope.selectTypecluster = function () {
                var url = serviceBase + "api/cluster/all";
                $http.get(url).success(function (response) {
                    $scope.selectcluster = response;
                });
            }
            $scope.selectTypecluster();

            $scope.cust = [];
            $scope.acust = function () {
                var url = serviceBase + "api/Notification/all";
                $http.get(url).success(function (response) {
                    $scope.cust = response;
                });
            }
            $scope.acust();
            $scope.selectedCatChanged = function (data) {
                $scope.cust = [];
                var url = serviceBase + "api/Notification/allfcmcust?id=" + data.id;
                $http.get(url)
                    .success(function (data) {
                        if (data.length == 0) {
                            alert("Not Found");
                        }
                        $scope.cust = data;

                    });
            };
            //for warehouse
            $scope.selectedWareChanged = function (data) {

                $scope.cust = [];
                var url = serviceBase + "api/Notification/allware?Warehouseid=" + data.WarehouseId + "&&" + "idd=1";
                $http.get(url)
                    .success(function (data) {
                        if (data.length == 0) {
                            alert("Not Found");
                        }
                        $scope.cust = data;

                    });
            }

            //for city
            $scope.selectedCityChanged = function (data) {

                $scope.cust = [];
                var url = serviceBase + "api/Notification/allcity?Cityid=" + data.Cityid;
                $http.get(url)
                    .success(function (data) {
                        if (data.length == 0) {
                            alert("Not Found");
                        }
                        $scope.cust = data;

                    });
            };

            //for cluster
            $scope.selectedClusterChanged = function (data) {

                //allcluster
                $scope.cust = [];
                var url = serviceBase + "api/Notification/allcluster?ClusterId=" + data.ClusterId;
                $http.get(url)
                    .success(function (data) {
                        if (data.length == 0) {
                            alert("Not Found");
                        }
                        $scope.cust = data;

                    });
            };
            $scope.examplemodel = [];

            $scope.exampledata = $scope.cust;
            $scope.examplesettings = {
                displayProp: 'ShopName', idProp: 'CustomerId',
                scrollableHeight: '300px',
                scrollableWidth: '450px',
                enableSearch: true,
                scrollable: true
            };
            $scope.selectedtypeChanged = function (data) {

                $scope.dataselect = [];
                var url = serviceBase + "api/Report/select?value=" + data.type;
                $http.get(url)
                    .success(function (data) {

                        if (data.length == 0) {
                            alert("Not Found");
                        }
                        $scope.dataselect = data;
                        console.log(data);
                    });
            };
            $scope.selectedItemChanged = function (data) {
                $scope.dataselect1 = [];
                var url = serviceBase + "api/Report/Catogory?value=" + data.value;
                $http.get(url)
                    .success(function (data) {
                        if (data.length == 0) {
                            alert("Not Found");
                        }
                        $scope.dataselect1 = data;
                        console.log(data);
                    });
            };
            $scope.Add = function (data) {

                var start = "";
                var end = "";
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                //if (!$('#dat').val()) {
                //    end = '';
                //    start = '';
                //    alert("Select Start and End Date")
                //    return;
                //}
                //else {
                start = f.val();
                end = g.val();
                // }

                var ids = [];
                _.each($scope.examplemodel, function (o2) {
                    console.log(o2);
                    for (var i = 0; i < $scope.cust.length; i++) {
                        if ($scope.cust[i].CustomerId == o2.id) {
                            var Row =
                            {
                                "id": o2.id
                            };
                            ids.push(Row);
                        }
                    }
                })
                var url = serviceBase + "api/Notification";
                var dataToPost = {

                    "Message": $scope.saveData.Message,
                    "NotifiedTo": $scope.saveData.NotifiedTo,
                    "CityName": $scope.saveData.CityName,
                    "ClusterName": $scope.saveData.ClusterName,
                    "WarehouseName": $scope.saveData.WarehouseName,
                    "NotificationByDeviceIdTime": $scope.saveData.NotificationTime,
                    //"Pic": serviceBase+"../../notificationimages/" + $scope.uploadedfileName,
                    "Pic": $scope.logourl,
                    "Title": $scope.saveData.title,
                    "From": start,
                    "TO": end,
                    ids: ids
                };

                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {
                        location.reload();
                    })
                    .error(function (data) {
                    })
            };
        }
    }
})();
