

(function () {
   // 'use strict';

    angular
        .module('app')
        .controller('AuthcustomerController', AuthcustomerController);

    AuthcustomerController.$inject = ['$scope', 'customerService', 'CityService', 'WarehouseService', "$filter", "$http", "ngTableParams", 'FileUploader', '$modal', '$log'];

    function AuthcustomerController($scope, customerService, CityService, WarehouseService, $filter, $http, FileUploader, ngTableParams, $modal, $log) {

        console.log("AuthcustomerController");

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,

                //locale: {
                format: 'MM/DD/YYYY h:mm A'
                //}
            });

        });

        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            console.log("get City");
            console.log(results.data);
            $scope.citys = results.data;
        }, function (error) {
            // alert("Not get city..");
        });

        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;

        }, function (error) {

        });

        //.................File Uploader method start..................

        $scope.uploadshow = true;
        $scope.toggle = function () {
            $scope.uploadshow = !$scope.uploadshow;
        }


        function sendFileToServer(formData, status) {
            var uploadURL = "/api/customerupload/post"; //Upload URL
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

        console.log("Vikash start");
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
            }
            this.setProgress = function (progress) {
                var progressBarWidth = progress * this.progressBar.width() / 100;
                this.progressBar.find('div').animate({ width: progressBarWidth }, 10).html(progress + "%&nbsp;");
                if (parseInt(progress) >= 100) {
                    this.abort.hide();
                }
            }
            this.setAbort = function (jqxhr) {
                var sb = this.statusbar;
                this.abort.click(function () {
                    jqxhr.abort();
                    sb.hide();
                });
            }
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

        $scope.customers = [];

        customerService.getcustomers().then(function (results) {
            //DataTransfer=JSON,
            $scope.customers = results.data;
            $scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });

        //............................Exel export Method.....................//

        $scope.currentPageStores = {};

        $scope.open = function (cust) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModalContent.html",
                    controller: "ModalInstanceCtrl1", resolve: { customer: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.currentPageStores.push(selectedItem);


                _.find($scope.currentPageStores, function (customer) {
                    if (customer.id == selectedItem.id) {
                        customer = selectedItem;
                    }
                });
                //$scope.currentPageStores = _.sortBy($scope.currentPageStores, 'CustomerId').reverse();
                //$scope.selected = selectedItem;
            },
                function () {
                    console.log("Cancel Condintion");
                    // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    $log.info("Modal dismissed at: " + new Date());
                })
        };


        $scope.customercategorys = {};


        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Open Dialog called");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerModaldelete.html",
                    controller: "ModalInstanceCtrldeleteCustomer", resolve: { customer: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);


                //$scope.tasktypes.push(selectedItem);

                //_.filter($scope.tasktypes, function (a) {

                //    if (a.id == selectedItem.id) {

                //        a.Name = selectedItem.Name;
                //        a.Desc = selectedItem.Desc;
                //    }

                //});

                //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                //$scope.selected = selectedItem;


            },
                function () {
                    console.log("Cancel Condintion");
                    // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //  $log.info("Modal dismissed at: " + new Date)
                })
        };

        $scope.edit = function (item) {
            console.log("Open Dialog called");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModalContentPut.html",
                    controller: "ModalInstanceCtrl1", resolve: { customer: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.customers.push(selectedItem);
                _.find($scope.customers, function (customer) {
                    if (customer.id == selectedItem.id) {
                        customer = selectedItem;
                    }
                });

                $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                $scope.selected = selectedItem;
                //$scope.customers.push(selectedItem);

                //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                //$scope.selected = selectedItem;


            },
                function () {
                    console.log("Cancel Condintion");
                    // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //  $log.info("Modal dismissed at: " + new Date)
                })
        };



        $scope.customers = [];

        customerService.getcustomers().then(function (results) {

            $scope.customers = results.data;
            $scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });
        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.customers;

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

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrl1', ModalInstanceCtrl1);

    ModalInstanceCtrl1.$inject = ["$scope", '$http', "$modalInstance", "customer", 'ngAuthSettings', 'WarehouseService', 'CustomerCategoryService'];

    function ModalInstanceCtrl1($scope, $http, $modalInstance, customer, ngAuthSettings, WarehouseService, CustomerCategoryService) {
        console.log("customer");




        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            console.log(results.data);
            console.log("data");
            $scope.warehouse = results.data;
        }, function (error) {
        });

        CustomerCategoryService.getcustomercategorys().then(function (results) {
            $scope.customercategorys = results.data;
            console.log($scope.customercategorys);
        }, function (error) {
            //alert(error.data.message);
        });
        $scope.CustomerData = {
            Name: '',
            Description: ''
        };
        if (customer) {
            $scope.CustomerData = customer;

            console.log($scope.CustomerData.Name);
            console.log($scope.CustomerData.Description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };



        $scope.AddCustomer = function (data) {


            console.log("Add Customer");
            var url = serviceBase + "api/customers";
            var dataToPost = {
                CustomerCategoryId: $scope.CustomerData.CustomerCategoryId,
                Name: $scope.CustomerData.Name,
                Description: $scope.CustomerData.Description,
                CustomerType: $scope.CustomerData.CustomerType,
                CustomerCategoryName: $scope.CustomerData.CustomerCategoryName,
                BillingAddress: $scope.CustomerData.BillingAddress,
                ShippingAddress: $scope.CustomerData.ShippingAddress,
                City: $scope.CustomerData.City,
                Mobile: $scope.CustomerData.Mobile,
                Warehouseid: $scope.CustomerData.Warehouseid,
                BAGPSCoordinates: $scope.CustomerData.BAGPSCoordinates,
                SAGPSCoordinates: $scope.CustomerData.SAGPSCoordinates,
                RefNo: $scope.CustomerData.RefNo,
                OfficePhone: $scope.CustomerData.OfficePhone,
                Emailid: $scope.CustomerData.Emailid,
                Familymember: $scope.CustomerData.Familymember,
                CreatedDate: $scope.CustomerData.CreatedDate,
                UpdatedDate: $scope.CustomerData.UpdatedDate,
                CreatedBy: $scope.CustomerData.CreatedBy,
                LastModifiedBy: $scope.CustomerData.LastModifiedBy,
                Active: $scope.CustomerData.Active
            };
            console.log("Data Added Succesfully");
            console.log(dataToPost);
            ////$("#spinner").show();

            //$http.post(url, dataToPost)
            //.success(function (ProjectData) {

            //    console.log("Error Gor Here");
            //    console.log(data);
            //    if (data[0].exception != null) {

            //        $scope.gotErrors = true;
            //        if (data[0].exception == "Already") {
            //            console.log("Got This User Already Exist");
            //            $scope.AlreadyExist = true;
            //        }

            //    }
            //    else {
            //        //console.log(data);
            //        //  console.log(data);
            //        $modalInstance.close(data[0]);
            //    }

            //})
            // .error(function (data) {
            //     console.log("Error Got Heere is ");
            //     console.log(data);

            //     // return $scope.showInfoOnSubmit = !0, $scope.revert()
            // })

            $http.post(url, dataToPost)
                .success(function (data) {

                    console.log("Error Gor Here in Customer");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                        //console.log(data);
                        //  console.log(data);
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })

        };


        $scope.PutCustomer = function (data) {
            $scope.CustomerData = {

            };
            if (customer) {
                $scope.CustomerData = customer;
                console.log("Puttt Customer");
                console.log(customer);
                console.log($scope.CustomerData);
                //console.log($scope.Customer.name);
                //console.log($scope.Customer.description);
            }

            console.log("Update Customer");
            var url = serviceBase + "api/customers";
            var dataToPost = {
                CustomerId: $scope.CustomerData.CustomerId,
                CustomerCategoryId: $scope.CustomerData.CustomerCategoryId,
                Name: $scope.CustomerData.Name,
                Description: $scope.CustomerData.Description,
                CustomerType: $scope.CustomerData.CustomerType,
                CustomerCategoryName: $scope.CustomerData.CustomerCategoryName,
                BillingAddress: $scope.CustomerData.BillingAddress,
                ShippingAddress: $scope.CustomerData.ShippingAddress,
                City: $scope.CustomerData.City,
                BAGPSCoordinates: $scope.CustomerData.BAGPSCoordinates,
                SAGPSCoordinates: $scope.CustomerData.SAGPSCoordinates,
                RefNo: $scope.CustomerData.RefNo,
                Mobile: $scope.CustomerData.Mobile,
                Warehouseid: $scope.CustomerData.Warehouseid,
                OfficePhone: $scope.CustomerData.OfficePhone,
                Emailid: $scope.CustomerData.Emailid,
                Familymember: $scope.CustomerData.Familymember,
                CreatedDate: $scope.CustomerData.CreatedDate,
                UpdatedDate: $scope.CustomerData.UpdatedDate,
                CreatedBy: $scope.CustomerData.CreatedBy,
                LastModifiedBy: $scope.CustomerData.LastModifiedBy,
                Active: $scope.CustomerData.Active
            };
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

                    console.log("Error Gor Here in Update Customer");
                    console.log(data);
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                        //console.log(data);
                        //  console.log(data);
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })

        };
        //$scope.PutCustomer = function (data) {
        //    console.log("Now Update click");
        //    console.log(customer);
        //    var postdata = "data";
        //    console.log("Being start execution");
        //    var url = serviceBase + "api/customers";
        //    console.log("Update Service is callng");
        //    $http.post(url, customer).success(function (data) {
        //        console.log("Success Get Sites Details");
        //        console.log(data);
        //        if (data.Data == "Success") {
        //            console.log("Cloase Popup");
        //            $modalInstance.close(data.updateData);
        //        }
        //        else {
        //            console.log("Error");

        //        }

        //    })

        //};


    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteCustomer', ModalInstanceCtrldeleteCustomer);

    ModalInstanceCtrldeleteCustomer.$inject = ["$scope", '$http', "$modalInstance", "customerService", 'ngAuthSettings', "customer"];

    function ModalInstanceCtrldeleteCustomer($scope, $http, $modalInstance, customerService, ngAuthSettings, customer) {
        console.log("delete modal opened");

        function ReloadPage() {
            location.reload();
        }




        $scope.customers = [];

        customerService.getcustomers().then(function (results) {

            $scope.customers = results.data;
            $scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });



        $scope.CustomerData = {

        };
        if (customer) {
            $scope.CustomerData = customer;
            console.log("found delete Customers");
            console.log(customer);
            console.log($scope.CustomerData);
            //console.log($scope.Customer.name);
            //console.log($scope.Customer.description);
        }


        //if (project) {
        //    console.log("Project if conditon");

        //    $scope.ProjectData = project;

        //    console.log($scope.ProjectData.ProjectName);
        //    //console.log($scope.ProjectData.Description);
        //}
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        //$scope.addTaskType = function (data) {
        //    console.log("add task type called");

        //    var url = serviceBase + "api/TaskTypes";
        //    var dataToPost = { id: 0, Name: data.Name, Desc: data.Desc };
        //    console.log(dataToPost);
        //    ////$("#spinner").show();

        //    $http.post(url, dataToPost)
        //    .success(function (data) {

        //        console.log("Error Gor Here");
        //        console.log(data);
        //        if (data.id == 0) {

        //            $scope.gotErrors = true;
        //            if (data[0].exception == "Already") {
        //                console.log("Got This User Already Exist");
        //                $scope.AlreadyExist = true;
        //            }

        //        }
        //        else {
        //            //console.log(data);
        //            //  console.log(data);
        //            $modalInstance.close(data);
        //        }

        //    })
        //     .error(function (data) {
        //         console.log("Error Got Heere is ");
        //         console.log(data);

        //         // return $scope.showInfoOnSubmit = !0, $scope.revert()
        //     })

        //};


        $scope.deletecustomers = function (dataToPost, index) {

            console.log("Delete customers");
            //alert(Id);
            //Id = window.encodeURIComponent(Id);

            customerService.deletecustomers(dataToPost).then(function (results) {
                console.log("Deleted Succesfully");

                //$scope.Deletecustomers.splice(index, 1);

                $modalInstance.close(dataToPost);
                //ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();