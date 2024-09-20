


(function () {
    //'use strict';

    angular
        .module('app')
        .controller('supplierController', supplierController);

    supplierController.$inject = ['$scope', 'FileUploader', 'supplierService', 'supplierCategoryService', "$filter", "$http", "ngTableParams", '$modal', 'CityService', "SubCategoryService"];

    function supplierController($scope, FileUploader, supplierService, supplierCategoryService, $filter, $http, ngTableParams, $modal, CityService, SubCategoryService) {
        $scope.validationPattern  = '^[a-zA-Z0-9._-]+$';

        console.log(" Supplier Controller reached");
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.uploadshow = true;
        $scope.toggle = function () {
            
            $scope.uploadshow = !$scope.uploadshow;
        }
        // supplier master History get data 

        $scope.MyCtrl = function (val) {

            

        }

        $scope.SupplierHistroy = function (data) {

            $scope.datasuppliermasterHistrory = [];
            var url = serviceBase + "api/Suppliers/supplierhistory?SupplierId=" + data.SupplierId;
            $http.get(url).success(function (response) {

                $scope.datasuppliermasterHistrory = response;
                console.log($scope.datasuppliermasterHistrory);
                $scope.AddTrack("View(History)", "SupplierId:", data.SupplierId);
            })
                .error(function (data) {
                })
        }


        //end supplier History
        //*****************************************Export Nullable Fileds**************************************//

    


        $scope.exportDataMissing = function () {

            var url = serviceBase + "api/Suppliers/Getmissingsupplier";
            $http.get(url).then(function (results) {

                $scope.stores = results.data;

                alasql('SELECT Name,CategoryName,PhoneNumber,Avaiabletime,Mobile,BillingAddress,ShippingAddress,TINNo,OfficePhone,EmailId,SalesManager,ContactPerson INTO XLSX("Supplier.xlsx",{headers:true}) FROM ?', [$scope.stores]);
            }, function (error) {
            });
        };



        $scope.cities = [];

        CityService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) { });





        $scope.SubCategorysV1 = [];

        $scope.SubCategorysV2 = function () {

            var url = serviceBase + "api/Suppliers/getBrandwithcategoryname";
            $http.get(url).then(function (results) {
                $scope.SubCategorysV1 = results.data;

            });

        };

        $scope.SubCategorysV2();


        //SubCategoryService.getsubcategorys().then(function (results) {
        //    $scope.SubCategorysV1 = results.data;
        //}, function (error) {
        //});

        $scope.GetSupplierCityWise = function (cityid, SubCaegoryId) {
            
            var url = serviceBase + "api/Suppliers/GetSuppliercityWiseAndbrandV1?cityid=" + cityid + "&SubCaegoryId=" + SubCaegoryId;
            $http.get(url)
                .success(function (data) {

                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    else {

                        $scope.supplier = data;
                        $scope.callmethod();
                        console.log(data);

                    }



                });
        }


        //*********************************************END*************************************************//
        function sendFileToServer(formData, status) {
            
            var uploadURL = "/api/supplierupload/post"; //Upload URL
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

        //*************************************************************************************************************//

        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData1 = function () {

            alasql('SELECT SupplierId,Name,SUPPLIERCODES,MobileNo,SubcategoryName,StateName,City,Active,CityPincode,ContactPerson,rating,CategoryName,Bank_Name,ManageAddress,BankPINno,Bank_AC_No,Bank_Ifsc,TINNo,FSSAI,BillingAddress,EmailId,WebUrl,ShopName,EstablishmentYear,StartedBusiness,bussinessType,PaymentTerms INTO XLSX("Full_Supplier_List.xlsx",{headers:true}) FROM ?', [$scope.stores]);
        };
        $scope.exportData = function () {
            alasql('SELECT * INTO XLSX("Supplier.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };

        //***************************************************************************************************************

        $scope.currentPageStores = {};

        $scope.open = function (supp) {
            
            console.log("Modal opened supplier");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    //backdrop: 'static',
                    templateUrl: "mySupplierModal.html",
                    controller: "ModalInstanceCtrlSupplier", resolve: { supplier: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        //$scope.open = function (supp) {
        //    
        //    window.location = "#/AddSupplier";
        //};

        $scope.edit = function (supplier) {
            console.log("Edit Dialog called supplier");
            var modalInstance;
            modalInstance = $modal.open(
                {
                   // backdrop: 'static',
                    templateUrl: "mySupplierModalPut.html",
                    controller: "ModalInstanceCtrlSupplier", resolve: { supplier: function () { return supplier } }
                });
            modalInstance.result.then(function (selectedsupplier) {
                $scope.supplier.push(selectedsupplier);
                _.find($scope.supplier, function (supplier) {
                    if (supplier.id == selectedsupplier.id) {
                        supplier = selectedsupplier;
                    }
                });
                $scope.supplier = _.sortBy($scope.supplier, 'Id').reverse();
                $scope.selected = selectedsupplier;
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };


        $scope.Activesupplier = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "activesuppliermodal.html",
                    controller: "Activesupplierctrl", resolve: { supplier: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.supplier.push(selectedsupplier);
                _.find($scope.supplier, function (supplier) {
                    if (supplier.id == selectedsupplier.id) {
                        supplier = selectedsupplier;
                    }
                });
                $scope.supplier = _.sortBy($scope.supplier, 'Id').reverse();
                $scope.selected = selectedsupplier;
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for supplier");

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteSupplier.html",
                    controller: "ModalInstanceCtrldeleteSupplier", resolve: { supplier: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        $scope.SupplierPayment = function (data) {


            localStorage.setItem("sample_data", JSON.stringify(data));

        };

        //to add depo "AddDepos"    //tejas 20-05-09
        $scope.AddDepo = function (dataaa) {
            
            console.log("Modal opened Add Depo ");
            $scope.adddepo = dataaa;
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myAddDepoModal.html",
                    controller: "AddDepoforSupplierController", resolve: { adddepos: function () { return $scope.adddepo } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        //View Depo by Anushka start
        $scope.OpenDepo = function (items) {

            console.log("open fn");
            $scope.depodata = items;
            supplierService.view(items).then(function (results) {

                $scope.GetDepoData();
                console.log("master Update fn");
                console.log(results);
            }, function (error) {
            });

        };

        $scope.supplier = [];

        var url = serviceBase + 'api/Suppliers/GetAllSupplierForUI';
        $http.get(url)
            .success(function (response) {

                $scope.supplier = response;
                console.log($scope.supplier);
                $scope.callmethod();
            });


        //supplierService.getsuppliers().then(function (results) {
        //    console.log("ingetfn");
        //    console.log(results.data);
        //    $scope.supplier = results.data;
        //    $scope.callmethod();
        //}, function (error) {
        //});


        //Anushka End 

        //Activate & Deactivate supplier  
        $scope.Activesupplier = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "activesuppliermodal.html",
                    controller: "Activesupplierctrl", resolve: { supplier: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.supplier.push(selectedsupplier);
                _.find($scope.supplier, function (supplier) {
                    if (supplier.id == selectedsupplier.id) {
                        supplier = selectedsupplier;
                    }
                });
                $scope.supplier = _.sortBy($scope.supplier, 'Id').reverse();
                $scope.selected = selectedsupplier;
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };
        // End Activate & Deactivate supplier  

        $scope.callmethod = function () {
            var init;

            $scope.stores = $scope.supplier;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";
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

            $scope.numPerPageOpt = [20, 50, 100, 200];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }

        var uploader = $scope.uploader = new FileUploader({

            url: serviceBase + 'api/logoUpload/UploadPeopleDocument'
        });

        //CALLBACKS

        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
            $scope.PeopleData.people.uploadedfileName = fileItem._file.name;
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
            $scope.PeopleData.Id_Proof = serviceBase + "../../UploadDocuments/" + fileItem._file.name;
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name, response);
            //$scope.uploadedfileName = fileItem._file.name, response;
            //$scope.uploadedfileName = fileItem._file.name, response;
            $scope.uploadedfileName(fileItem._file.name, response);
        };
        uploader.onCompleteAll = function () {
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader);


        $scope.SupplierPayment = function (supplier) {

            console.log("Modal opened supplier");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mySupplierPaymentModel.html",
                    controller: "paymentsupplierctrl", resolve: { supplier: function () { return supplier } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");

                });
        };

    }
})();
//hier

(function () {
    //'use strict';

    angular
        .module('app')

        .directive('replace', function () {
            return {
                require: 'ngModel',
                scope: {
                    regex: '@replace',
                    with: '@with'
                },
                link: function (scope, element, attrs, model) {
                    model.$parsers.push(function (val) {
                        if (!val) { return; }
                        var regex = new RegExp(scope.regex);
                        var replaced = val.replace(regex, scope.with);
                        if (replaced !== val) {
                            model.$setViewValue(replaced);
                            model.$render();
                        }
                        return replaced;
                    });
                }
            };
        })

   

   })();



(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlSupplier', ModalInstanceCtrlSupplier);
    ModalInstanceCtrlSupplier.$inject = ["$scope", '$http', 'ngAuthSettings', "supplierService", 'supplierCategoryService', "$modalInstance", "supplier", "CityService", "StateService", "SubCategoryService", 'FileUploader'];
    function ModalInstanceCtrlSupplier($scope, $http, ngAuthSettings, supplierService, supplierCategoryService, $modalInstance, supplier, CityService, StateService, SubCategoryService, FileUploader) {

        
        console.log("supplier");
        $scope.DisableSupplierbutton = false;
        var today = new Date();
        $scope.today = today.toISOString();
        $scope.Buyer = {};
        $scope.SupplierData = {

        };

        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                    console.log("buyers : " + $scope.Buyer);

                });

        };
        $scope.getpeople();

        $scope.GSTInfo = [];      
        $scope.uploadGSTImage = {GSTImage: 'File' };

        $scope.FSSAIInfo = [];      
        $scope.uploadFSSAIImage = { FSSAIImage: 'File' };

        $scope.PanCardInfo = [];        
        $scope.uploadPanCardImage = { PanCardImage: 'File' };
        
        $scope.CancelChequeInfo = [];      
        $scope.uploadCancelChequeImage = { CancelCheque: 'File' };





        supplierCategoryService.getsupplierCategory().then(function (results) {
            $scope.supplierCategorys = results.data;
        }, function (error) {
        });
        if (supplier) {           
            $scope.SupplierData = supplier;
            console.log($scope.SupplierData);           
        }

        $scope.citys = [];

        $scope.GetCityAdd = function (data) {

            var url = serviceBase + 'api/City/suppliercity?Statid=' + data;
            $http.get(url)
                .success(function (response) {

                    $scope.citysAdd = response;

                });
        };



        $scope.subcat = [];

        $scope.Getsubcat = function (data) {
            var url = serviceBase + 'api/SubCategory/GetSubCategory?CatId=' + data;
            $http.get(url)
                .success(function (response) {

                    $scope.subcat = response;

                });
        };




        $scope.states = [];
        StateService.getstates().then(function (results) {          
            $scope.states = results.data;
            //var url = serviceBase + "api/City/GetByIdd?Cityid"; //change because active agent show
            //$http.get(url)
            //    .success(function (data) {
            //        
            //        $scope.Gagent = data;
            //    });
        }, function (error) {


        });


        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;

        }, function (error) {
        });


        $scope.GetCityEdit = function (data) {

            var url = serviceBase + 'api/City/suppliercity?Statid=' + data;
            $http.get(url)
                .success(function (response) {

                    $scope.citys = response;

                });
        };


        $scope.SubCategorys = [];
        //SubCategoryService.getsubcategorys().then(function (results) {
        //    $scope.dataselect = results.data;
        //}, function (error) {
        //    });



        $scope.SubCategorysV3 = function () {

            var url = serviceBase + "api/Suppliers/getBrandwithcategoryname";
            $http.get(url).then(function (results) {
                $scope.dataselect = results.data;

            });

        };

        $scope.SubCategorysV3();
        $scope.MultiBrandModel = [];
        $scope.MultiBrand = $scope.dataselect;
        $scope.MultiBrandModelsettings = {
            displayProp: 'SubcategoryName', idProp: 'SubCategoryId',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddSupplier = function (data) {
            
            $scope.DisableSupplierbutton = true;
            if (data.Name == null || data.Name == "") {

                alert('Please Enter Name');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.Cityid === null || data.Cityid === "") {
                alert('Please Enter CityID');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.City === null || data.City === "") {
                alert('Please Enter City');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.CityPincode === null || data.CityPincode === "") {
                alert('Please Enter CityPincode');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.Stateid === null || data.Stateid === "") {
                alert('Please Enter Stateid');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.StateName === null || data.StateName === "") {
                alert('Please Enter StateName');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.EstablishmentYear == null || data.EstablishmentYear == "") {
                alert('Please Enter ShopNameestablishmentyear ');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.bussinessType == null || data.bussinessType == "") {
                alert('Please Enter bussinessType ');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            //else if (data.Description == null || data.Description == "") {
            //    alert('Please Enter description ');
            //    $modalInstance.open();
            //}
            else if (data.TINNo == null || data.TINNo == "") {
                alert('Please Enter TINNo');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }

            else if (data.Pancard === null || data.Pancard === "") {
                alert('Please Enter PANCard');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }

            else if (data.StartedBusiness == null || data.StartedBusiness == "") {
                alert('Please Enter startedbusiness ');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.FSSAI == null || data.FSSAI == "") {
                alert('Please Enter FSSSAl ');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.MobileNo == null || data.MobileNo == "") {
                alert('Please Enter MobileNo');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.EmailId == null || data.EmailId == "") {
                alert('Please Enter EmailId');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.ManageAddress == null || data.ManageAddress == "") {
                alert('Please Enter ManageAddress ');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.Bank_Name == null || data.Bank_Name == "") {
                alert('Please Enter Bank Name');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.Bank_AC_No == null || data.Bank_AC_No == "") {
                alert('Please Enter Bank Account ');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.Bank_Ifsc == null || data.Bank_Ifsc == "") {
                alert('Please Enter IFSC Code');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.BankPINno == null || data.BankPINno == "") {
                alert('Please Enter  Bank Pin Code');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }

            else if (data.SupplierCaegoryId == null || data.SupplierCaegoryId == "") {
                alert('Please Enter Category');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }

            else if (data.WebUrl == null || data.WebUrl == "") {
                alert('Please Enter WebUrl');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.rating == null || data.rating == "") {
                alert('Please Enter rating');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.BillingAddress == null || data.BillingAddress == "") {
                alert('Please Enter BillingAddress');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }

            else if (data.SUPPLIERCODES == null || data.SUPPLIERCODES == "") {
                alert('Please Enter SUPPLIERCODES');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.ContactPerson == null || data.ContactPerson == "") {
                alert('Please Enter ContactPerson');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }

            else if (data.PaymentTerms == null || data.PaymentTerms == "") {
                alert('Please Enter PaymentTerms');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }
            else if (data.Password == null || data.Password == "") {
                alert('Please Enter Password ');
                $scope.DisableSupplierbutton = false;
                $modalInstance.open();
            }

            //else if (data.Name.length > 0) {
               
            //    var letters = /^[A-Za-z]+$/;
            //    if (data.Name.match(letters)) {
                    
            //    }
            //    else {
            //        alert('Please input alphabet characters only');
            //        return false;
            //    }
            //}

            console.log("Supplier");

            var url = serviceBase + "api/Suppliers";

            var dataToPost = {
                IsStopAdvancePr: $scope.SupplierData.IsStopAdvancePr,
                IsIRNInvoiceRequired: $scope.SupplierData.IsIRNInvoiceRequired,
                //ContactImage: $scope.SupplierData.ContactImage,
                Password: $scope.SupplierData.Password,
                SupplierId: $scope.SupplierData.SupplierId,
                SupplierCaegoryId: $scope.SupplierData.SupplierCaegoryId,
                Cityid: $scope.SupplierData.Cityid,
                City: $scope.SupplierData.City,
                CityPincode: $scope.SupplierData.CityPincode,
                Stateid: $scope.SupplierData.Stateid,
                CibilScore: $scope.SupplierData.CibilScore,
                StateName: $scope.SupplierData.StateName,
                PeopleID: $scope.SupplierData.PeopleId,
                CategoryName: $scope.SupplierData.CategoryName,
                Name: $scope.SupplierData.Name,
                EstablishmentYear: $scope.SupplierData.EstablishmentYear,
                //OpeningHours: $scope.SupplierData.OpeningHours,
                Pancard: $scope.SupplierData.Pancard,
                bussinessType: $scope.SupplierData.bussinessType,
                //Description: $scope.SupplierData.Description,
                TINNo: $scope.SupplierData.TINNo,
                StartedBusiness: $scope.SupplierData.StartedBusiness,
                FSSAI: $scope.SupplierData.FSSAI,
                MobileNo: $scope.SupplierData.MobileNo,
                EmailId: $scope.SupplierData.EmailId,
                ManageAddress: $scope.SupplierData.ManageAddress,
                Bank_Name: $scope.SupplierData.Bank_Name,
                Bank_AC_No: $scope.SupplierData.Bank_AC_No,
                Bank_Ifsc: $scope.SupplierData.Bank_Ifsc,
                BankPINno: $scope.SupplierData.BankPINno,
                BillingAddress: $scope.SupplierData.BillingAddress,
                OfficePhone: $scope.SupplierData.OfficePhone,
                Id_Proof: $scope.SupplierData.Id_Proof,
                WebUrl: $scope.SupplierData.WebUrl,
                SUPPLIERCODES: $scope.SupplierData.SUPPLIERCODES,
                ContactPerson: $scope.SupplierData.ContactPerson,
                PaymentTerms: data.PaymentTerms,
                rating: $scope.SupplierData.rating,
                ImageUrl: $scope.SupplierData.ImageUrl,
                DepoId: $scope.SupplierData.DepoId,
                DepoName: $scope.SupplierData.DepoName,
                ShopName: $scope.SupplierData.ShopName,
                SubBrandid: $scope.MultiBrandModel,
                GSTImage: $scope.GSTImage,
                FSSAIImage: $scope.FSSAIImage,
                PanCardImage: $scope.PanCardImage,
                CancelCheque: $scope.CancelCheque 

            };
           
            $http.post(url, dataToPost)
           
                .success(function (data) {

                    
                    if (data) {

                        alert("Supplier created Successfully");
                     
                        window.location.reload();

                    }
                    else {
                        alert("Something Went Wrong");
                        $modalInstance.close(data);
                        window.location.reload();
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
           
        };


        //$scope.SubCategorys = [];
        //SubCategoryService.getsubcategorys().then(function (results) {
        //    $scope.dataselect = results.data;
        //}, function (error) {
        //});

        //$scope.MultiBrandModel = [];
        //$scope.MultiBrand = $scope.dataselect;
        //$scope.MultiBrandModelsettings = {
        //    displayProp: 'SubcategoryName', idProp: 'SubCategoryId',
        //    scrollableHeight: '450px',
        //    scrollableWidth: '550px',
        //    enableSearch: true,
        //    scrollable: true
        //};

        $('.agentmultiselect').multiselect();
        $scope.dataselect = [];


        var url = serviceBase + "api/Suppliers/GetBrandEditSupplier?SupplierId=" + $scope.SupplierData.SupplierId;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                $scope.vm.dataselect = data;
                $scope.vm.examplemodel = $scope.vm.dataselect.filter(function (elem) {
                    return elem.Selected;
                });
            });
        $scope.mySplit = function (string, nb) {
            var array = string.split(',');
            return array;
        }
        $scope.vm = {};

        $scope.vm.dataselect = [];
        $scope.vm.examplemodel = [$scope.vm.dataselect[0], $scope.vm.dataselect[2]];
        $scope.exampledata = $scope.dataselect;
        $scope.examplesettings1 =
            {
                displayProp: 'name', idProp: 'id',
                scrollableHeight: '300px',
                scrollableWidth: '450px',
                enableSearch: true,
                scrollable: true
            };



        $scope.PutSupplier = function (data) {
            

            if (data.Name == null || data.Name == "") {
                alert('Please Enter Name');
                $modalInstance.open();
            }
            else if (data.Name.length > 0) {
                
                var letters = /^[A-Za-z ]+$/;
                if (data.Name.match(letters)) {
                   
                }
                else {
                    alert('Please input alphabet characters only');
                    return false;
                }
            }
            else if (data.TINNo == null || data.TINNo == "") {
                alert('Please Enter TINNo');
                $modalInstance.open();
            }
            else if (data.FSSAI == null || data.FSSAI == "") {
                alert('Please Enter FSSSAl ');
                $modalInstance.open();
            }
            else if (data.MobileNo == null || data.MobileNo == "") {
                alert('Please Enter MobileNo');
                $modalInstance.open();
            }
            else if (data.EmailId == null || data.EmailId == "") {
                alert('Please Enter EmailId');
                $modalInstance.open();
            }
            else if (data.ManageAddress == null || data.ManageAddress == "") {
                alert('Please Enter ManageAddress ');
                $modalInstance.open();
            }

            else if (data.Pancard === null || data.Pancard === "") {
                alert('Please Enter PANCard');
                $modalInstance.open();
            }
            //else if (data.OpeningHours === null || data.OpeningHours === "") {
            //    alert('Please Enter OpeningHours');
            //    $modalInstance.open();
            //}

            else if (data.CityPincode === null || data.CityPincode === "") {
                alert('Please Enter CityPincode');
                $modalInstance.open();
            }

            else if (data.Bank_Name == null || data.Bank_Name == "") {
                alert('Please Enter Bank Name');
                $modalInstance.open();
            }
            else if (data.Bank_AC_No == null || data.Bank_AC_No == "") {
                alert('Please Enter Bank Account ');
                $modalInstance.open();
            }
            else if (data.Bank_Ifsc == null || data.Bank_Ifsc == "") {
                alert('Please Enter IFSC Code');
                $modalInstance.open();
            }
            else if (data.BankPINno == null || data.BankPINno == "") {
                alert('Please Enter  Bank Pin Code');
                $modalInstance.open();
            }
            else if (data.BillingAddress == null || data.BillingAddress == "") {
                alert('Please Enter BillingAddress');
                $modalInstance.open();
            }
            $scope.SupplierData = {

            };
            if (supplier) {
                $scope.SupplierData = supplier;
                console.log("found Put Supplier");
                console.log(supplier);
                console.log($scope.SupplierData);
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


            console.log("Update Supplier");


            var url = serviceBase + "api/Suppliers/put";

            //  alert($scope.SupplierData.SupplierCaegoryId);
            var dataToPost = {
                ////SupplierId: $scope.SupplierData.SupplierId,
                ////SupplierCaegoryId: $scope.SupplierData.SupplierCaegoryId,
                ////CategoryName: $scope.SupplierData.CategoryName,
                ////Name: $scope.SupplierData.Name,
                ////PeopleID: $scope.SupplierData.PeopleID,
                ////Avaiabletime: $scope.SupplierData.Avaiabletime,
                ////PhoneNumber: $scope.SupplierData.PhoneNumber,
                ////BillingAddress: $scope.SupplierData.BillingAddress,
                ////ShippingAddress: $scope.SupplierData.ShippingAddress,
                ////Comments: $scope.SupplierData.Comments,
                ////TINNo: $scope.SupplierData.TINNo,
                ////OfficePhone: $scope.SupplierData.OfficePhone,
                ////MobileNo: $scope.SupplierData.MobileNo,
                ////EmailId: $scope.SupplierData.EmailId,
                ////WebUrl: $scope.SupplierData.WebUrl,
                ////SUPPLIERCODES: $scope.SupplierData.SUPPLIERCODES,
                ////ContactPerson: $scope.SupplierData.ContactPerson,
                ////PaymentTerms: data.PaymentTerms,
                ////rating: $scope.SupplierData.rating,
                ////ImageUrl: $scope.SupplierData.ImageUrl,
                ////DepoId: $scope.SupplierData.DepoId,
                ////DepoName: $scope.SupplierData.DepoName,
                ////ShopName: $scope.SupplierData.ShopName,
                ////EstablishmentYear: $scope.SupplierData.EstablishmentYear,
                ////bussinessType: $scope.SupplierData.bussinessType,
                ////Description: $scope.SupplierData.Description,
                ////StartedBusiness: $scope.SupplierData.StartedBusiness,
                ////FSSAI: $scope.SupplierData.FSSAI,
                ////ManageAddress: $scope.SupplierData.ManageAddress,
                //ManageAddress: $scope.SupplierData.ManageAddress,

                //////ContactImage: $scope.SupplierData.ContactImage,
                Password: $scope.SupplierData.Password,
                SupplierId: $scope.SupplierData.SupplierId,
                CibilScore: $scope.SupplierData.CibilScore,

                SupplierCaegoryId: $scope.SupplierData.SupplierCaegoryId,
                Cityid: $scope.SupplierData.Cityid,
                City: $scope.SupplierData.City,
                IsStopAdvancePr: $scope.SupplierData.IsStopAdvancePr,
                IsIRNInvoiceRequired: $scope.SupplierData.IsIRNInvoiceRequired,
                //BrandId: $scope.SupplierData.BrandId,
                //Brand: $scope.SupplierData.Brand,
                CityPincode: $scope.SupplierData.CityPincode,
                Stateid: $scope.SupplierData.Stateid,
                StateName: $scope.SupplierData.StateName,
                PeopleID: $scope.SupplierData.PeopleId,
                CategoryName: $scope.SupplierData.CategoryName,
                Name: $scope.SupplierData.Name,
                EstablishmentYear: $scope.SupplierData.EstablishmentYear,

                bussinessType: $scope.SupplierData.bussinessType,
                //Description: $scope.SupplierData.Description,
                TINNo: $scope.SupplierData.TINNo,
                StartedBusiness: $scope.SupplierData.StartedBusiness,
                FSSAI: $scope.SupplierData.FSSAI,
                MobileNo: $scope.SupplierData.MobileNo,
                EmailId: $scope.SupplierData.EmailId,
                //OpeningHours: $scope.SupplierData.OpeningHours,
                Pancard: $scope.SupplierData.Pancard,
                ManageAddress: $scope.SupplierData.ManageAddress,
                Bank_Name: $scope.SupplierData.Bank_Name,
                Bank_AC_No: $scope.SupplierData.Bank_AC_No,
                Bank_Ifsc: $scope.SupplierData.Bank_Ifsc,
                BankPINno: $scope.SupplierData.BankPINno,
                ImageUrl: $scope.SupplierData.ImageUrl,
                BillingAddress: $scope.SupplierData.BillingAddress,
                //  Avaiabletime: $scope.SupplierData.Avaiabletime,
                //PhoneNumber: $scope.SupplierData.PhoneNumber,
                //BillingAddress: $scope.SupplierData.BillingAddress,
                //ShippingAddress: $scope.SupplierData.ShippingAddress,
                //Comments: $scope.SupplierData.Comments,

                OfficePhone: $scope.SupplierData.OfficePhone,

                WebUrl: $scope.SupplierData.WebUrl,
                SUPPLIERCODES: $scope.SupplierData.SUPPLIERCODES,
                ContactPerson: $scope.SupplierData.ContactPerson,
                PaymentTerms: $scope.SupplierData.PaymentTerms,
                rating: $scope.SupplierData.rating,

                DepoId: $scope.SupplierData.DepoId,
                DepoName: $scope.SupplierData.DepoName,
                ShopName: $scope.SupplierData.ShopName,


                SubBrandid: $scope.vm.examplemodel

            };

            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
                    if (data.SupplierId > 0)
                    {
                        
                         alert("Supplier Update Successfully");
                           
                        $modalInstance.close(data);

                    }
                    else
                    {
                        alert("Something went wrong");

                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })
        };

        //first Image GST Uploader

        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/Suppliers/DocumentImageUpload'
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
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = "GST" + fileExtension;
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
            $scope.GSTInfo.uploadedfileName = fileItem._file.name;
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
            $scope.GSTImage = response.slice(1, -1); //For remove

            $scope.GSTInfo.push($scope.GSTImage);

            alert("Image Uploaded Successfully");
        };
        uploader.onCompleteAll = function () {
            
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader);

        //second Image FSSAI Uploader

        var uploader1 = $scope.uploader1 = new FileUploader({
            url: serviceBase + 'api/Suppliers/DocumentImageUpload'
        });
        //FILTERS

        uploader1.filters.push({

            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader1.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader1.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = "FSSAI" + fileExtension;
        };
        uploader1.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader1.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader1.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader1.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader1.onSuccessItem = function (fileItem, response, status, headers) {

            console.info('onSuccessItem', fileItem, response, status, headers);
            $scope.FSSAIInfo.uploadedfileName = fileItem._file.name;
        };
        uploader1.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader1.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader1.onCompleteItem = function (fileItem, response, status, headers) {

            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
            $scope.FSSAIImage = response.slice(1, -1); //For remove
            
            $scope.FSSAIInfo.push($scope.FSSAIImage);

            alert("Image Uploaded Successfully");
        };
        uploader1.onCompleteAll = function () {
            
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader1);

//----------------------------------------------------
        //Third Image Pancard Uploader

        var uploader2 = $scope.uploader2 = new FileUploader({
            url: serviceBase + 'api/Suppliers/DocumentImageUpload'
        });
        //FILTERS

        uploader2.filters.push({

            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader2.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader2.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = "Pancard" + fileExtension;
        };
        uploader2.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader2.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader2.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader2.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader2.onSuccessItem = function (fileItem, response, status, headers) {

            console.info('onSuccessItem', fileItem, response, status, headers);
            $scope.PanCardInfo.uploadedfileName = fileItem._file.name;
        };
        uploader2.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader2.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader2.onCompleteItem = function (fileItem, response, status, headers) {

            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
            $scope.PanCardImage = response.slice(1, -1); //For remove
            
            $scope.PanCardInfo.push($scope.PanCardImage);

            alert("Image Uploaded Successfully");
        };
        uploader2.onCompleteAll = function () {
            
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader2);






        //----------------------------------------------------
        //Forth Image Cancel cheque Uploader

        var uploader3 = $scope.uploader3 = new FileUploader({
            url: serviceBase + 'api/Suppliers/DocumentImageUpload'
        });
        //FILTERS

        uploader3.filters.push({

            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader3.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader3.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = "CancelCheque" + fileExtension;
        };
        uploader3.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader3.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader3.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader3.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader3.onSuccessItem = function (fileItem, response, status, headers) {

            console.info('onSuccessItem', fileItem, response, status, headers);
            $scope.CancelChequeInfo.uploadedfileName = fileItem._file.name;
        };
        uploader3.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader3.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader3.onCompleteItem = function (fileItem, response, status, headers) {

            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
            $scope.CancelCheque = response.slice(1, -1); //For remove
            
            $scope.CancelChequeInfo.push($scope.CancelCheque);

            alert("Image Uploaded Successfully");
        };
        uploader3.onCompleteAll = function () {
            
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader3);




    }

})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AddDepoforSupplierController', AddDepoforSupplierController);

    AddDepoforSupplierController.$inject = ["$scope", '$http', 'TaxGroupService', 'CityService', 'StateService', "$modalInstance", "adddepos", 'FileUploader'];

    function AddDepoforSupplierController($scope, $http, TaxGroupService, CityService, StateService, $modalInstance, adddepos ,FileUploader) {
        //add depos with respect to Supplier ID -- tejas 20-05-2019
        $scope.Depodisablebutton = false;
        var input = document.getElementById("file");
        console.log(input);
        
        $scope.DepoData = adddepos;
        $scope.DepoData.PRPOStopAfterValue = 3;
        $scope.supplierid = $scope.DepoData.SupplierId;

        var today = new Date();
        $scope.today = today.toISOString();

        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });

        console.log("Depo");
        $scope.DepoData = {};


        //$scope.states = [];
        //$scope.GetState = function () {
        //    var url = serviceBase + 'api/States/supplierstate';
        //    $http.get(url)
        //        .success(function (response) {

        //            $scope.states = response;
        //        });
        //};
        //$scope.GetState();




        $scope.GSTInfo = [];
        
        $scope.uploadGSTImage = { GSTImage: 'File' };

        $scope.FSSAIInfo = [];
        
        $scope.uploadFSSAIImage = { FSSAIImage: 'File' }

        $scope.PanCardInfo = [];
        
        $scope.uploadPanCardImage = { PanCardImage: 'File' };

        $scope.CancelChequeInfo = [];
        
        $scope.uploadCancelChequeImage = { CancelCheque: 'File' };
        $scope.DepoData.PRPOStopAfterValue = 3;




        $scope.states = [];
        StateService.getstates().then(function (results) {
            console.log("sumit");
            console.log(results.data);
            $scope.states = results.data;
        });

        $scope.citys = [];
        $scope.GetCity = function (data) {

            var url = serviceBase + 'api/City/suppliercity?Statid=' + data;
            $http.get(url)
                .success(function (response) {

                    $scope.citys = response;

                });
        };
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddDepo = function (data) {
            
            $scope.DisableDepoButton = true;
            
            
            if (data.DepoName == null || data.DepoName == "") {
                alert('Please Enter Name');
                $scope.DisableDepoButton = false;
                $modalInstance.open();
            }

            if (data.GSTin == null || data.GSTin == "") {
                alert('Please Enter GSTin ');
                $scope.DisableDepoButton = false;
                $modalInstance.open();
            }

            else if (data.Address == null || data.Address == "") {
                alert('Please Enter address');
                $modalInstance.open();
            }
            else if (data.Email == null || data.Email == "") {
                alert('Please Enter Email');
                $scope.DisableDepoButton = false;
                $modalInstance.open();
            }
            else if (data.Phone == null || data.Phone == "") {
                alert('Please Enter Phone number');
                $scope.DisableDepoButton = false;
                $modalInstance.open();
            }

            else if (data.Stateid == null || data.Stateid == "") {
                alert('Please Select State ');
                $scope.DisableDepoButton = false;
                $modalInstance.open();
            }
            else if (data.Cityid == null || data.Cityid == "") {
                alert('Please Select  City');
                $scope.DisableDepoButton = false;
                $modalInstance.open();
            }
            else if (data.ContactPerson == null || data.ContactPerson == "") {
                alert('Please Enter  Contact Person');
                $scope.DisableDepoButton = false;
                $modalInstance.open();
            }
            else if (data.PRPOStopAfterValue == null || data.PRPOStopAfterValue == "" || data.PRPOStopAfterValue == 0) {
                alert('Please Enter  PR Po Stop Value');
                $scope.DisableDepoButton = false;
                $scope.DepoData.PRPOStopAfterValue = 3;
                $modalInstance.open();
               
            }

            var url = serviceBase + "api/Suppliers/AddDepos";
            var dataToPost = {
                //DepoId: $scope.DepoData.DepoId,
                SupplierId: $scope.supplierid,
                DepoName: $scope.DepoData.DepoName,
                GSTin: $scope.DepoData.GSTin,
                DepoCodes: $scope.DepoData.DepoCodes,
                Address: $scope.DepoData.Address,
                Email: $scope.DepoData.Email,
                Phone: $scope.DepoData.Phone,
                Stateid: $scope.DepoData.Stateid,
                Cityid: $scope.DepoData.Cityid,
                ContactPerson: $scope.DepoData.ContactPerson,
                //--tejas
                FSSAI: $scope.DepoData.FSSAI,
                CityPincode: $scope.DepoData.CityPincode,
                Bank_Name: $scope.DepoData.Bank_Name,
                Bank_AC_No: $scope.DepoData.Bank_AC_No,
                BankAddress: $scope.DepoData.BankAddress,
                Bank_Ifsc: $scope.DepoData.Bank_Ifsc,
                BankPinCode: $scope.DepoData.BankPinCode,
                PANCardNo: $scope.DepoData.PANCardNo,
                OpeningHours: $scope.DepoData.OpeningHours,
                //--tejas
                CreatedDate: $scope.DepoData.CreatedDate,
                UpdatedDate: $scope.DepoData.UpdatedDate,
                CreatedBy: $scope.DepoData.CreatedBy,
                UpdateBy: $scope.DepoData.UpdateBy,
                IsActive: $scope.DepoData.IsActive,
                GSTImage: $scope.GSTImage, 
                FSSAIImage: $scope.FSSAIImage, 
                PanCardImage: $scope.PanCardImage,
                CancelCheque: $scope.CancelCheque,
                PRPOStopAfterValue: $scope.DepoData.PRPOStopAfterValue
            };
            console.log("kkkkkk");
            console.log(dataToPost);
            $("#AddDepoID").prop("disabled", true)
            $http.post(url, dataToPost)
                .success(function (data) {

                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                            $scope.Depodisablebutton = false;
                        }
                    }
                    else {
                        $scope.DisableDepoButton = true;
                        alert("Depo created Successfully");
                       
                        //console.log(data);
                        //  console.log(data);
                        $modalInstance.close(data);
                    }
                })
                .error(function (data, status) {
                    //console.log("Error Got Here is ");
                    //console.log(data);
                    alert(data.ErrorMessage);
                    return false;
                })
        };



        //first Image GST Uploader

        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/Suppliers/DepoDocumentImageUpload'
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
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = "DepoGST" + fileExtension ;
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
            $scope.GSTInfo.uploadedfileName = fileItem._file.name;
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
            $scope.GSTImage = response.slice(1, -1); //For remove

            $scope.GSTInfo.push($scope.GSTImage);

            alert("Image Uploaded Successfully");
        };
        uploader.onCompleteAll = function () {
            
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader);

        //second Image FSSAI Uploader

        var uploader1 = $scope.uploader1 = new FileUploader({
            url: serviceBase + 'api/Suppliers/DepoDocumentImageUpload'
        });
        //FILTERS

        uploader1.filters.push({

            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader1.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader1.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = "DepoFSSAI" + fileExtension;
        };
        uploader1.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader1.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader1.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader1.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader1.onSuccessItem = function (fileItem, response, status, headers) {

            console.info('onSuccessItem', fileItem, response, status, headers);
            $scope.FSSAIInfo.uploadedfileName = fileItem._file.name;
        };
        uploader1.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader1.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader1.onCompleteItem = function (fileItem, response, status, headers) {

            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
            $scope.FSSAIImage = response.slice(1, -1); //For remove
            
            $scope.FSSAIInfo.push($scope.FSSAIImage);

            alert("Image Uploaded Successfully");
        };
        uploader1.onCompleteAll = function () {
            
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader1);

        //----------------------------------------------------
        //Third Image Pancard Uploader

        var uploader2 = $scope.uploader2 = new FileUploader({
            url: serviceBase + 'api/Suppliers/DepoDocumentImageUpload'
        });
        //FILTERS

        uploader2.filters.push({

            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader2.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader2.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = "DepoPancard" + fileExtension;
        };
        uploader2.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader2.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader2.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader2.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader2.onSuccessItem = function (fileItem, response, status, headers) {

            console.info('onSuccessItem', fileItem, response, status, headers);
            $scope.PanCardInfo.uploadedfileName = fileItem._file.name;
        };
        uploader2.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader2.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader2.onCompleteItem = function (fileItem, response, status, headers) {

            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
            $scope.PanCardImage = response.slice(1, -1); //For remove
            
            $scope.PanCardInfo.push($scope.PanCardImage);

            alert("Image Uploaded Successfully");
        };
        uploader2.onCompleteAll = function () {
            
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader2);






        //----------------------------------------------------
        //Forth Image Cancel cheque Uploader

        var uploader3 = $scope.uploader3 = new FileUploader({
            url: serviceBase + 'api/Suppliers/DepoDocumentImageUpload'
        });
        //FILTERS

        uploader3.filters.push({

            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader3.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader3.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = "DepoCancelCheque" + fileExtension; 
        };
        uploader3.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader3.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader3.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader3.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader3.onSuccessItem = function (fileItem, response, status, headers) {

            console.info('onSuccessItem', fileItem, response, status, headers);
            $scope.CancelChequeInfo.uploadedfileName = fileItem._file.name;
        };
        uploader3.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader3.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader3.onCompleteItem = function (fileItem, response, status, headers) {

            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
            $scope.CancelCheque = response.slice(1, -1); //For remove
            
            $scope.CancelChequeInfo.push($scope.CancelCheque);

            alert("Image Uploaded Successfully");
        };
        uploader3.onCompleteAll = function () {
            
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader3);

        



    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteSupplier', ModalInstanceCtrldeleteSupplier);

    ModalInstanceCtrldeleteSupplier.$inject = ["$scope", '$http', "$modalInstance", "supplierService", 'ngAuthSettings', "supplier"];

    function ModalInstanceCtrldeleteSupplier($scope, $http, $modalInstance, supplierService, ngAuthSettings, supplier) {
        console.log("delete modal opened");
        $scope.suppliers = [];
        function ReloadPage() {
            location.reload();
        }

        if (supplier) {
            $scope.SupplierData = supplier;
            console.log("found supplier");
            console.log(supplier);
            console.log($scope.SupplierData);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.deletesuppliers = function (dataToPost, $index) {
            console.log("Delete  supplier controller");
            supplierService.deletesuppliers(dataToPost).then(function (results) {
                console.log("Del");
                $modalInstance.close(dataToPost);
                //ReloadPage();
            }, function (error) {
                alert(error.data.message);
            });
        };

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlSupplierPayment', ModalInstanceCtrlSupplierPayment);

    ModalInstanceCtrlSupplierPayment.$inject = ["$scope", '$http', "$modalInstance", "supplierService", 'ngAuthSettings', "supplier", "$filter"];

    function ModalInstanceCtrlSupplierPayment($scope, $http, $modalInstance, supplierService, ngAuthSettings, supplier, $filter) {
        console.log("Open Supplier Payment");

        if (supplier) {

            $scope.SupplierData = supplier;
            console.log("found supplier");
            console.log(supplier);
            console.log($scope.SupplierData);
        }
        $(function () {

            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });
        $(function () {

            $('input[name="daterangedata"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.SupplierPayment = function (data) {
        };
        function GetPaymentData() {
            var url = serviceBase + "api/Suppliers/supplierPaymentData?SupplierId=" + $scope.SupplierData.SupplierId + "&WarehouseId=" + $scope.SupplierData.WarehouseId;
            $http.get(url).success(function (response) {

                $scope.SupplierPaymentData = response;
                $scope.callmethod();
                console.log($scope.SupplierPaymentData);
            })
                .error(function (data) {
                });
        }
        GetPaymentData();

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.SupplierPaymentData;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";


            $scope.numPerPageOpt = [20, 50, 100, 200];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        };
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
        .controller('Activesupplierctrl', Activesupplierctrl);

    Activesupplierctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "supplier", 'FileUploader'];

    function Activesupplierctrl($scope, $http, ngAuthSettings, $modalInstance, supplier, FileUploader) {
        console.log("Supplier");

        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();
        $scope.$watch('files', function () {
            //$scope.upload($scope.files);
        });
        ////for image

        $scope.SupplierData = supplier;
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.ActSupplier = function (data) {
            $scope.loogourl = supplier.LogoUrl;
            console.log("Update Supplier");
            var url = serviceBase + "api/Suppliers/Activate";
            var dataToPost = {
                SupplierId: data.SupplierId,
                Active: data.Active,
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
                })


        };

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('paymentsupplierctrl', paymentsupplierctrl);

    paymentsupplierctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "supplier", 'FileUploader'];

    function paymentsupplierctrl($scope, $http, ngAuthSettings, $modalInstance, supplier, FileUploader) {

        console.log("Supplier");

        var input = document.getElementById("file");
        var today = new Date();
        $scope.poids = [];
        $scope.today = today.toISOString();
        $scope.$watch('files', function () {
            //$scope.upload($scope.files);
        });
        ////for image

        $scope.SupplierData = supplier;
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.addSupplierPayment = function (data) {

            console.log(data);
            var url = serviceBase + "api/Suppliers/addSupplierPayment";
            //  alert($scope.SupplierData.SupplierCaegoryId);
            var dataToPost = {
                SupplierId: $scope.SupplierData.SupplierId,
                Amount: data.Amount,
                Cityid: $scope.SupplierData.Cityid,
                POId: data.PoId,
                WarehouseId: $scope.SupplierData.WarehouseId


            };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
                    if (data !== null) {
                        alert("Supplier Payment add successful.");
                        window.location.reload();
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
        $scope.getPoId = function () {

            var url = serviceBase + 'api/Suppliers/GetPoId?supplierId=' + $scope.SupplierData.SupplierId;
            $http.get(url)
                .success(function (response) {

                    $scope.poids = response;

                });
        };
        $scope.getPoId();
    }
})();





