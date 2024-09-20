
(function () {
  

    angular
        .module('app')
        .controller('WarehousecategoryController', WarehousecategoryController);

    WarehousecategoryController.$inject = ['$scope', 'WarehouseCategoryService', 'CityService', 'StateService', "WarehouseService", "SubsubCategoryService", "CategoryService", "$filter", "$http", "ngTableParams", '$modal'];

    function WarehousecategoryController($scope, WarehouseCategoryService, CityService, StateService, WarehouseService, SubsubCategoryService, CategoryService, $filter, $http, ngTableParams, $modal) {
        console.log(" category Controller reached");
        $scope.currentPageStores = {};
        $scope.uploadshow = true;
        $scope.toggle = function () {
            $scope.uploadshow = !$scope.uploadshow;
        };

        $scope.warehouse = [];
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;

                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;

                $scope.getData($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.getWarehosues();
        function sendFileToServer(formData, status) {

            var uploadURL = "/api/WarehouseCategoryUpload/post"; //Upload URL
            var extraData = {}; //Extra Data.
            var jqXHR = $.ajax({
                xhr: function () {
                    var xhrobj = $.ajaxSettings.xhr();
                    if (xhrobj.upload) {
                        xhrobj.upload.addEventListener('progress', function (event) {
                            var percent = 0;
                            var position = event.loaded || event.position;
                            var total = event.total;
                            if (event.lengthComputable) {
                                percent = Math.ceil(position / total * 100);
                            }
                            //Set progress
                            status.setProgress(percent);
                        }, false);
                    }
                    return xhrobj;
                },
                url: uploadURL,
                type: "POST",
                contentType: false,
                processData: false,
                cache: false,
                data: formData,
                success: function (data) {
                    status.setProgress(100);

                    $("#status1").append("Data from Server:" + data + "<br>");
                }
            });

            status.setAbort(jqXHR);
        }

        var rowCount = 0;
        function createStatusbar(obj) {
            rowCount++;
            var row = "odd";
            if (rowCount % 2 == 0) row = "even";
            this.statusbar = $("<div class='statusbar " + row + "'></div>");
            this.filename = $("<div class='filename'></div>").appendTo(this.statusbar);
            this.size = $("<div class='filesize'></div>").appendTo(this.statusbar);
            this.progressBar = $("<div class='progressBar'><div></div></div>").appendTo(this.statusbar);
            this.abort = $("<div class='abort'>Abort</div>").appendTo(this.statusbar);
            obj.after(this.statusbar);

            this.setFileNameSize = function (name, size) {
                var sizeStr = "";
                var sizeKB = size / 1024;
                if (parseInt(sizeKB) > 1024) {
                    var sizeMB = sizeKB / 1024;
                    sizeStr = sizeMB.toFixed(2) + " MB";
                }
                else {
                    sizeStr = sizeKB.toFixed(2) + " KB";
                }

                this.filename.html(name);
                this.size.html(sizeStr);
            };
            this.setProgress = function (progress) {
                var progressBarWidth = progress * this.progressBar.width() / 100;
                this.progressBar.find('div').animate({ width: progressBarWidth }, 10).html(progress + "%&nbsp;");
                if (parseInt(progress) >= 100) {
                    this.abort.hide();
                }
            };
            this.setAbort = function (jqxhr) {
                var sb = this.statusbar;
                this.abort.click(function () {
                    jqxhr.abort();
                    sb.hide();
                });
            };
        }
        function handleFileUpload(files, obj) {
            for (var i = 0; i < files.length; i++) {
                var fd = new FormData();
                fd.append('file', files[i]);
                var status = new createStatusbar(obj); //Using this we can set progress.
                status.setFileNameSize(files[i].name, files[i].size);
                sendFileToServer(fd, status);

            }
        }
        $(document).ready(function () {
            var obj = $("#dragandrophandler");
            obj.on('dragenter', function (e) {
                e.stopPropagation();
                e.preventDefault();
                $(this).css('border', '2px solid #0B85A1');
            });
            obj.on('dragover', function (e) {
                e.stopPropagation();
                e.preventDefault();
            });
            obj.on('drop', function (e) {

                $(this).css('border', '2px dotted #0B85A1');
                e.preventDefault();
                var files = e.originalEvent.dataTransfer.files;
                $("body").on("drop", "#dragandrophandler", function () {
                    var allowedFiles = [".xlsx"];
                    var fileUpload = $("#fileUpload");
                    var lblError = $("#lblError");
                    var regex = new RegExp("([a-zA-Z0-9\s_\\.\-:])+(" + allowedFiles.join('|') + ")$");
                    if (!regex.test(fileUpload.val().toLowerCase())) {
                        lblError.html("Please upload files having extensions: <b>" + allowedFiles.join(', ') + "</b> only.");
                        return false;
                    }
                    lblError.html('');
                    return true;
                });
                //We need to send dropped files to Server
                handleFileUpload(files, obj);
            });
            $(document).on('dragenter', function (e) {
                e.stopPropagation();
                e.preventDefault();
            });
            $(document).on('dragover', function (e) {
                e.stopPropagation();
                e.preventDefault();
                obj.css('border', '2px dotted #0B85A1');
            });
            $(document).on('drop', function (e) {
                e.stopPropagation();
                e.preventDefault();
            });

            //$("body").on("drop", "#dragandrophandler", function () {
            //    var allowedFiles = [".xlsx"];
            //    var fileUpload = $("#fileUpload");
            //    var lblError = $("#lblError");
            //    var regex = new RegExp("([a-zA-Z0-9\s_\\.\-:])+(" + allowedFiles.join('|') + ")$");
            //    if (!regex.test(fileUpload.val().toLowerCase())) {
            //        lblError.html("Please upload files having extensions: <b>" + allowedFiles.join(', ') + "</b> only.");
            //        return false;
            //    }
            //    lblError.html('');
            //    return true;
            //});

        });

        //............................File Uploader Method End.....................//

        //............................Exel export Method.....................//

        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData1 = function () {
            alasql('SELECT Name,Description,Address,CustomerCategoryName,CreatedDate INTO XLSX("Customer.xlsx",{headers:true}) FROM ?', [$scope.stores]);
        };
        $scope.exportData = function () {
            alasql('SELECT * INTO XLSX("Customer.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };


        $scope.currentPageStores = {};

        ///----------------------------Mapping Warehouse Based ------------------------------////
        $scope.WarhouseStates = [];
        WarehouseService.getwarehousedistinctstates().then(function (results) {

            console.log("This is warehouse state");
            console.log(results.data);
            $scope.WarhouseStates = results.data;
        }, function (error) {
        });

        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            console.log("This is city in city master");
            $scope.citys = results.data;
        }, function (error) {
        });

        $scope.warehouses = []
        WarehouseService.getwarehouse().then(function (results) {
            console.log("This is Warehouse name in Warehouse master");
            $scope.warehouses = results.data;
            console.log($scope.warehouses);

        }, function (error) {

        });


        $scope.WhCategoryData = {};



        $scope.WhCategorynew = [];
        $scope.whcount = 0;
        $scope.GetWarhouseCategory = function (WareHouse) {

            console.log(WareHouse);

            console.log("get Warehousecategory controller");
            console.log(WareHouse);

            $scope.WhCategoryold = [];
            $scope.WhCategoryAll = [];
            $scope.Wcategorys = [];

            CategoryService.getWarhouseCategory(WareHouse).then(function (results) {

                console.log(results.data);
                console.log("get Warehousecategory Wcategorys");
                $scope.Wcategorys = results.data;
                // $scope.GetCategory();

            }, function (error) { })
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            $scope.checkAll = function () {

                if ($scope.selectedAll) {
                    $scope.selectedAll = false;
                } else {
                    $scope.selectedAll = true;
                }
                angular.forEach($scope.Wcategorys, function (category) {
                    $scope.checkboxes.items[category.Categoryid] = $scope.selectedAll;
                });

            };

        $scope.checkboxes = { 'checked': false, items: {} };
        $scope.$watch('checkboxes.checked', function (value) {

            angular.forEach($scope.Wcategorys, function (category) {
                if (angular.isDefined(category.Categoryid)) {
                    $scope.checkboxes.items[category.Categoryid] = value;
                }
            });
        });


        $scope.$watch('checkboxes.items', function (values) {

            if (!$scope.Wcategorys) {
                return;
            }
            var checked = 0, unchecked = 0,
                total = $scope.Wcategorys.length;
            angular.forEach($scope.Wcategorys, function (category) {
                checked += ($scope.checkboxes.items[category.Categoryid]) || 0;
                unchecked += (!$scope.checkboxes.items[category.Categoryid]) || 0;
            });
            if ((unchecked == 0) || (checked == 0)) {
                $scope.checkboxes.checked = (checked == total);
            }
            angular.element(document.getElementById("select_all")).prop("indeterminate", (checked != 0 && unchecked != 0));
        }, true);



        $scope.AddWhCategory = function () {

            $("#po").prop("disabled", true);
            var modalInstance;
            $scope.CkdId = [];
            var strItms = JSON.stringify($scope.checkboxes.items);
            strItms = strItms.replace("{", "");
            strItms = strItms.replace("}", "");
            var data = strItms.split(",");
            for (var i = 0; i < data.length; i++) {
                data[i] = data[i].replace("\"", "");
                var strData = data[i].split("\":");
                var id = strData[0];
                var value = strData[1];
                if (value == "true") {
                    $scope.CkdId.push(id);
                }
            }
            $scope.plist = [];
            for (var j = 0; j < $scope.CkdId.length; j++) {
                _.each($scope.Wcategorys, function (o2) {
                    if (o2.Categoryid == $scope.CkdId[j]) {
                        $scope.plist.push(o2);
                    }
                })
                // $scope.PurchaseList = _.reject($scope.PurchaseList, function (o2) { return o2.OrderDetailsId == $scope.CkdId[j]; });
            }

            console.log($scope.WhCategoryold);
            var url = serviceBase + "api/WarehouseCategory";

            var dataToPost = $scope.plist;

            console.log("DATA:");
            console.log(dataToPost);

            dataToPost[0].Warehouseid = $scope.WhCategoryData.WarehouseId;
            console.log(dataToPost);

            $http.post(url, dataToPost)
                .success(function (data) {
                    alert('Successfully Mapped');
                    location.reload();
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
                        ReloadPage();
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };

        //----------------------Mapping End Warehouse Based-------------------------///


        $scope.whcategorys = [];
        $scope.getData = function (WarehouseId) {

            WarehouseCategoryService.getwhcategoryswid(WarehouseId).then(function (results) {

                $scope.whcategorys = results.data;
                $scope.callmethod();
            }, function (error) {

            });
        }

        $scope.Activecate = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "activewarehousecmodal.html",
                    controller: "WarehouseCategoryeditctrl", resolve: { whcategory: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.whcategorys.push(selectedItem);
                    _.find($scope.whcategorys, function (whcategory) {
                        if (whcategory.id == selectedItem.id) {
                            whcategory = selectedItem;
                        }
                    });
                    $scope.whcategorys = _.sortBy($scope.whcategorys, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };


        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.whcategorys,

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
                });
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('WarehouseCategoryeditctrl', WarehouseCategoryeditctrl);

    WarehouseCategoryeditctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "whcategory", 'FileUploader'];

    function WarehouseCategoryeditctrl($scope, $http, ngAuthSettings, $modalInstance, whcategory, FileUploader) {
        console.log("category");

        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();
        $scope.$watch('files', function () {
            //$scope.upload($scope.files);
        });
        ////for image

        $scope.CategoryData = whcategory;
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.putwarehousecategory = function (data) {
                $scope.loogourl = whcategory.LogoUrl;
                console.log("Update category");

                var url = serviceBase + "api/WarehouseCategory/WHCatAct";
                var dataToPost = {

                    WhCategoryid: data.WhCategoryid,
                    IsActive: data.IsActive,

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
                    });


            };
    }
})();







