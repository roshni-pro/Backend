'use strict';
app.controller('itemMasterController', ['$scope', 'itemMasterService', 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader', function ($scope, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {
    var UserRole = JSON.parse(localStorage.getItem('RolePerson'));


    function sendFileToServer(formData, status) {

        //var fdata = new FormData();
        formData.append("WareHouseId", $scope.Warehouseid);
        formData.append("compid", $scope.compid);

        var uploadURL = "/api/ItemMasterUpload/post"; //Upload URL
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
            // data: fdata,
            success: function (data) {
                status.setProgress(100);
                $("#status1").append("Data from Server: " + data + "<br>");
                //alert("Succesfully Submitted...........");               
            },
        });
        status.setAbort(jqXHR);
    }
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

    $scope.DDWarhouseId = {};
    $scope.Warehouseid = 0;
    $scope.centralButton = false;
    $scope.WarehouseButton = false;
    //User Tracking
    //$scope.AddTrack = function (Atype, page, Detail) {

    //    console.log("Tracking Code");
    //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
    //    $http.post(url).success(function (results) { });
    //}
    //End User Tracking
    $scope.deactivated = function (warehid) {

        $scope.deactivate = [];
        $scope.deactivate1 = [];

        $http.get(serviceBase + "api/itemMaster/ItemMasterDeactivatedList?warehouseid=" + warehid)

            .success(function (data) {

                $scope.deactivate = data;
                //for (var i = 0; i < $scope.deactivate.length; i++) {
                //    $scope.deactivate1.push($scope.deactivate[i].ItemListDeactivatedDetails);
                //}

                alasql('SELECT CityName,Cityid,CategoryName, CategoryCode,SubcategoryName,SubsubcategoryName,BrandCode,itemname,itemcode,ItemMultiMRPId,Number,SellingSku,price,PurchasePrice,UnitPrice,MinOrderQty,SellingUnitName,PurchaseMinOrderQty,StoringItemName,PurchaseSku,PurchaseUnitName,SupplierName,SUPPLIERCODES,BaseCategoryName,TGrpName,TotalTaxPercentage,WarehouseName,HindiName,SizePerUnit,Barcode,[Active],[Deleted],Margin,PromoPoint,HSNCode,IsSensitive INTO XLSX("ItemsD.xlsx",{headers:true}) FROM ?', [$scope.deactivate]);
            })


            .error(function (data) {
            })
    };

    $scope.deactivatednew = function (warehidnew) {

        $scope.deactivatenew = [];
        $scope.deactivate1 = [];

        $http.get(serviceBase + "api/itemMaster/ItemMasterDeactivatedList10?warehouseid=" + warehidnew)

            .success(function (data) {

                $scope.deactivatenew = data;
                //for (var i = 0; i < $scope.deactivate.length; i++) {
                //    $scope.deactivate1.push($scope.deactivate[i].ItemListDeactivatedDetails);
                //}

                alasql('SELECT CityName,Cityid,CategoryName, CategoryCode,SubcategoryName,SubsubcategoryName,BrandCode,itemname,itemcode,ItemMultiMRPId,Number,SellingSku,price,PurchasePrice,UnitPrice,MinOrderQty,SellingUnitName,PurchaseMinOrderQty,StoringItemName,PurchaseSku,PurchaseUnitName,SupplierName,SUPPLIERCODES,BaseCategoryName,TGrpName,TotalTaxPercentage,WarehouseName,HindiName,SizePerUnit,Barcode,[Active],[Deleted],Margin,PromoPoint,HSNCode,IsSensitive INTO XLSX("ItemsD.xlsx",{headers:true}) FROM ?', [$scope.deactivatenew]);
            })


            .error(function (data) {
            })
    };

    if (UserRole.rolenames.includes('HQ')) {

        $scope.warehouse = [];
        //$scope.warehousename = "W1";
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.compid = $scope.UserRole.compid;
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {

                $scope.warehouse = results.data;
                //$scope.Warehouseid = $scope.warehouse[0].WarehouseId;
                $scope.centralButton = true;


                $scope.getData1($scope.pageno);
            }, function (error) {
            })
        };
        $scope.getWarehosues(); 


        $scope.status = 'Default';
        $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [50, 100, 200];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData1($scope.pageno);
        }

        $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown

        $scope.getData1 = function (pageno) { // This would fetch the data on page change.



            if ($scope.WarehouseButton == true) {

                $scope.getDataWarehouse(pageno);
            } else {

                if ($scope.pagenoOne != pageno) {
                    $scope.pagenoOne = pageno;
                    $scope.itemMasters = [];

                    var url = serviceBase + "api/itemMaster" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&status=" + $scope.status;
                    $http.get(url).success(function (response) {
                        $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                        console.log("get current Page items:");
                        console.log($scope.itemMasters);
                        $scope.total_count = response.total_count;
                        $scope.currentPageStores = $scope.itemMasters;
                        //$scope.AddTrack("View", "ItemMaster:", "");
                    });
                }

            }
        };

        //missingHSNCode
        $scope.missingHSNCode = function () {

            var url = serviceBase + "api/itemMaster/getitemMaster/";
            $http.get(url).success(function (response) {
                $scope.currentPageStores = response;
            });
        };
        //end

        //missingHSNCodeCentral
        $scope.missingHSNCodeCentral = function () {

            var url = serviceBase + "api/itemMaster/getitemMastercentral/";
            $http.get(url).success(function (response) {
                $scope.currentPageStores = response;
            });
        };
        //end
        $scope.missingDetail = function () {

            var url = serviceBase + "api/itemMaster/getmissingdetail/";
            $http.get(url).success(function (response) {
                $scope.currentPageStores = response;
            });
        };

        $scope.search = function () {

            var url = serviceBase + "api/itemMaster/Searchinitemat?key=" + $scope.searchKeywords;
            $http.get(url).success(function (response) {
                $scope.currentPageStores = response;
            });
        };

        $scope.refresh = function () {
            $scope.currentPageStores = $scope.itemMasters;
            $scope.pagenoOne = 0;
            $scope.getData1($scope.pageno);
        };

        ////.................File Uploader method start..................
        $scope.uploadshow = true;

        $scope.toggle = function () {
            $scope.uploadshow = !$scope.uploadshow;
        }

        var rowCount = 0;

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

        //............................Exel export Method.....................//

        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData1 = function () {

            $scope.stores = [];

            $http.get(serviceBase + "api/itemMaster/export?warehouseid=" + $scope.Warehouseid)

                .success(function (data) {
                    $scope.stores = data;
                    alasql('SELECT CityName,Cityid,CategoryName, CategoryCode,SubcategoryName,SubsubcategoryName,BrandCode,itemname,itemcode,ItemMultiMRPId,Number,SellingSku,price,PurchasePrice,UnitPrice,MinOrderQty,SellingUnitName,PurchaseMinOrderQty,StoringItemName,PurchaseSku,PurchaseUnitName,SupplierName,SUPPLIERCODES,BaseCategoryName,TGrpName,TotalTaxPercentage,WarehouseName,HindiName,SizePerUnit,Barcode,[Active],[Deleted],Margin,PromoPoint,HSNCode,IsSensitive INTO XLSX("Items.xlsx",{headers:true}) FROM ?', [$scope.stores]);
                })


                .error(function (data) {
                })
        };
        $scope.exportCentralItemData = function () {

            $scope.stores = [];

            $http.get(serviceBase + "api/itemMaster/exportCentral")
                .success(function (data) {
                    $scope.stores = data;
                    alasql('SELECT CategoryName, CategoryCode,SubcategoryName,SubsubcategoryName,BrandCode,itemname,itemcode,Number,SellingSku,price,PurchasePrice,UnitPrice,MinOrderQty,SellingUnitName,PurchaseMinOrderQty,StoringItemName,PurchaseSku,PurchaseUnitName,BaseCategoryName,TGrpName,TotalTaxPercentage,HindiName,SizePerUnit,Barcode,[Active],[Deleted],Margin,PromoPoint,HSNCode,IsSensitive INTO XLSX("Items.xlsx",{headers:true}) FROM ?', [$scope.stores]);
                })
                .error(function (data) {
                })
        };
        $scope.exportData = function () {

            alasql('SELECT * INTO XLSX("Items.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };
        $scope.currentPageStores = {};
        $scope.update = function () {

            if ($scope.selectedItem == "TextBox") {
            }
            else if ($scope.selectedItem == "RadioButton") {
            }
            else if ($scope.selectedItem == "MultiSelect") {
            }
        }

        $scope.open = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myitemMasterModal.html",
                    controller: "ModalInstanceCtrladditem", resolve: { itemMaster: function () { return $scope.items } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                    })
        };

        $scope.edit = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myitemMasterPut.html",
                    controller: "ModalInstanceCtrlitemMaster", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };

        //MultiMrp Harry 30/03/2019
        $scope.openMultiMRP = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "addMultiplePriceModal.html",
                    controller: "ModalInstanceCtrladdMultiplePrice", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                    })
        };

        $scope.SetActive = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myactivemodal.html",
                    controller: "ModalInstanceCtrlitemMaster", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {

                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };
        $scope.SetFree = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myfreemodal.html",
                    controller: "ModalInstanceCtrlitemMaster", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {

                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };

        $scope.opendelete = function (data, $index) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteitemMaster.html",
                    controller: "ModalInstanceCtrldeleteitemMaster", resolve: { itemMaster: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                    })
        };

        $scope.getWareHouseStores = function () {


            $scope.pagenoOne = 0;
            $scope.pageno = 1;
            $scope.total_count = 0;
            $scope.getDataWarehouse($scope.pageno);
        };



        $scope.getDataWarehouse = function (pageno) { // This would fetch the data on page change.

            $scope.centralButton = false;
            $scope.WarehouseButton = true;
            if ($scope.pagenoOne != pageno) {
                $scope.pagenoOne = pageno;
                $scope.itemMasters = [];
                var url = serviceBase + "api/itemMaster/Getwarehouseitems" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&status=" + $scope.status;
                $http.get(url).success(function (response) {
                    $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                    $scope.total_count = response.total_count;
                    $scope.currentPageStores = $scope.itemMasters;
                });
            }
        };

        //...Export Warehouse Current Item List...Export function.....//

        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        };
        $scope.exportDataStatus = function () {
            var url = serviceBase + "api/itemMaster/GetwarehouseitemsExport" + "?Warehouseid=" + $scope.Warehouseid + "&status=" + $scope.status;
            $http.get(url).success(function (response) {
                $scope.exportItems = response.ordermaster;
                alasql('SELECT CityName,Cityid,CategoryName, CategoryCode,SubcategoryName,SubsubcategoryName,BrandCode,itemname,itemcode,Number,SellingSku,price,PurchasePrice,UnitPrice,MinOrderQty,SellingUnitName,PurchaseMinOrderQty,StoringItemName,PurchaseSku,PurchaseUnitName,SupplierName,SUPPLIERCODES,BaseCategoryName,TGrpName,TotalTaxPercentage,WarehouseName,HindiName,SizePerUnit,Barcode,[active],[Deleted],Margin,PromoPoint,HSNCode,CurrentStock INTO XLSX("Items.xlsx",{headers:true}) FROM ?', [$scope.exportItems]);
            });

        };

        //END

        $scope.oldprice = function (data) {

            $scope.dataoldprice = [];
            var url = serviceBase + "api/itemMaster/oldprice?Number=" + data.Number;
            $http.get(url).success(function (response) {
                $scope.dataoldprice = response;
                console.log($scope.dataoldprice);
                //$scope.AddTrack("View(History)", "ItemNme:", data.itemname);
            })
                .error(function (data) {
                })
        }


        $scope.WareHouseBysearch = function () {

            $scope.pagenoOne = 0;
            $scope.pageno = 1;
            $scope.total_count = 0;
            var url = serviceBase + "api/itemMaster/WHSearchinitematAdmin?key=" + $scope.searchKeywords + "&warehouseid=" + $scope.Warehouseid;
            $http.get(url).success(function (response) {
                $scope.currentPageStores = response;

            });
        };


        $scope.activDInactive = function (Warehouseid) {
            
            if (Warehouseid >= 0) {
                
                $scope.pagenoOne = 0;
                $scope.getDataWarehouse($scope.pageno);
            } else {
                
                $scope.pagenoOne = 0;
                $scope.getData1($scope.pageno);
            }
            $scope.pagenoOne = 0;

        }



        $scope.Whedit = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myWhitemMasterPut.html",
                    controller: "ModalInstanceCtrlWhitemMaster", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };
        $scope.WhSetActive = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myWhactivemodal.html",
                    controller: "ModalInstanceCtrlWhitemMaster", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {

                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };

        $scope.WhAddItemLimit = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myWhAddLimitmodal.html",
                    controller: "ModalInstanceCtrlWhAddItemLimit", resolve: { itemLimit: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.itemLimit.push(selectedItem);
                    _.find($scope.itemLimit, function (itemLimit) {

                        if (itemLimit.id == selectedItem.id) {
                            itemLimit = selectedItem;
                        }
                    });
                    $scope.itemLimit = _.sortBy($scope.itemLimit, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };

        $scope.Wholdprice = function (data) {

            $scope.dataoldprice = [];
            var url = serviceBase + "api/itemMaster/Woldprice?ItemId=" + data.ItemId;
            $http.get(url).success(function (response) {
                $scope.dataoldprice = response;
                console.log($scope.dataoldprice);
            })
                .error(function (data) {
                })
        }
        $scope.Whopendelete = function (data, $index) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModalWhdeleteitemMaster.html",
                    controller: "ModalInstanceCtrlWhdeleteitemMaster", resolve: { itemMaster: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                    })
        };


        //ItemLimitQty Anushka 05/06/2019
        $scope.openItemLimit = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myWhAddLimitmodal.html",
                    controller: "ModalInstanceCtrlWhAddItemLimit", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                    })
        };

        //set color for active & inactive
        $scope.set_color = function (data) {

            if (data.active == true) {
                return { background: "lightgreen", color: "black" }
            }
            else if (data.active == false) {
                return { background: "#e94b3b", color: "white" }
            }
            else {
            }
        }


        //openAddImage Pooja 10/07/2019
        $scope.openAddImage = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "addItemImageModal.html",
                    controller: "ModalInstanceCtrladdItemImage", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                    })
        };

    }
    else if (UserRole.rolenames.includes('WH')) {

        $scope.warehouse = [];
        // $scope.warehousename = "W1";
        $scope.Warehouseid = 1;
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.compid = $scope.UserRole.compid;
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
                $scope.getData1($scope.pageno);
            }, function (error) {
            })
        };
        $scope.getWarehosues();
        
        $scope.status = 'Default';
        $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [50, 100, 200];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData1($scope.pageno);
        }
        $scope.activDInactive = function () {
            
            $scope.pagenoOne = 0;
            $scope.getData1($scope.pageno);
        }

        $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown

        $scope.getData1 = function (pageno) { // This would fetch the data on page change.
            

            $scope.WarehouseButton = true;

            if ($scope.pagenoOne != pageno) {
                $scope.pagenoOne = pageno;
                $scope.itemMasters = [];
                $scope.WhcurrentPageStores = [];

                var url = serviceBase + "api/itemMaster/Getwarehouseitems" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&status=" + $scope.status;
                $http.get(url).success(function (response) {
                    $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                    console.log("get current Page items:");
                    console.log($scope.itemMasters);
                    $scope.total_count = response.total_count;
                    $scope.WhcurrentPageStores = $scope.itemMasters;
                });
            }
        };

        //$scope.WareHouseBysearch = function () {
        //    $scope.WhcurrentPageStores = [];
        //    var url = serviceBase + "api/itemMaster/WHSearchinitemat?key=" + $scope.searchKeywords;
        //    $http.get(url).success(function (response) {

        //        $scope.WhcurrentPageStores = response;
        //    });
        //};
        $scope.WareHouseBysearch = function () {
            
            $scope.WhcurrentPageStores = [];
            var url = serviceBase + "api/itemMaster/WHSearchinitematAdmin?key=" + $scope.searchKeywords + "&warehouseid=" + $scope.Warehouseid;
            $http.get(url).success(function (response) {
                $scope.WhcurrentPageStores = response;

            });
        };





        $scope.refresh = function () {
            $scope.currentPageStores = $scope.itemMasters;
            $scope.pagenoOne = 0;
            $scope.getData1($scope.pageno);
        };

        ////.................File Uploader method start..................
        $scope.uploadshow = true;

        $scope.toggle = function () {
            $scope.uploadshow = !$scope.uploadshow;
        }

        var rowCount = 0;
        //function createStatusbar(obj) {
        //    rowCount++;
        //    var row = "odd";
        //    if (rowCount % 2 == 0) row = "even";
        //    this.statusbar = $("<div class='statusbar " + row + "'></div>");
        //    this.filename = $("<div class='filename'></div>").appendTo(this.statusbar);
        //    this.size = $("<div class='filesize'></div>").appendTo(this.statusbar);
        //    this.progressBar = $("<div class='progressBar'><div></div></div>").appendTo(this.statusbar);
        //    this.abort = $("<div class='abort'>Abort</div>").appendTo(this.statusbar);
        //    obj.after(this.statusbar);
        //    this.setFileNameSize = function (name, size) {
        //        var sizeStr = "";
        //        var sizeKB = size / 1024;
        //        if (parseInt(sizeKB) > 1024) {
        //            var sizeMB = sizeKB / 1024;
        //            sizeStr = sizeMB.toFixed(2) + " MB";
        //        }
        //        else {
        //            sizeStr = sizeKB.toFixed(2) + " KB";
        //        }
        //        this.filename.html(name);
        //        this.size.html(sizeStr);
        //    }
        //    this.setProgress = function (progress) {
        //        var progressBarWidth = progress * this.progressBar.width() / 100;
        //        this.progressBar.find('div').animate({ width: progressBarWidth }, 10).html(progress + "%&nbsp;");
        //        if (parseInt(progress) >= 100) {
        //            this.abort.hide();
        //        }
        //    }
        //    this.setAbort = function (jqxhr) {
        //        var sb = this.statusbar;
        //        this.abort.click(function () {
        //            jqxhr.abort();
        //            sb.hide();
        //        });
        //    }
        //}
        //function handleFileUpload(files, obj) {
        //    for (var i = 0; i < files.length; i++) {
        //        var fd = new FormData();
        //        fd.append('file', files[i]);
        //        var status = new createStatusbar(obj); //Using this we can set progress.
        //        status.setFileNameSize(files[i].name, files[i].size);
        //        sendFileToServer(fd, status);
        //    }
        //}
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

        //............................Exel export Method.....................//

        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData1 = function () {

            $scope.stores = [];


            $http.get(serviceBase + "api/itemMaster/export?warehouseid=" + $scope.Warehouseid)
                .success(function (data) {
                    $scope.stores = data;
                    alasql('SELECT CityName,Cityid,CategoryName, CategoryCode,SubcategoryName,SubsubcategoryName,BrandCode,itemname,itemcode,Number,SellingSku,price,PurchasePrice,UnitPrice,MinOrderQty,SellingUnitName,PurchaseMinOrderQty,StoringItemName,PurchaseSku,PurchaseUnitName,SupplierName,SUPPLIERCODES,BaseCategoryName,TGrpName,TotalTaxPercentage,WarehouseName,HindiName,SizePerUnit,Barcode,[Active],[Deleted],Margin,PromoPoint,HSNCode INTO XLSX("Items.xlsx",{headers:true}) FROM ?', [$scope.stores]);
                })
                .error(function (data) {
                })

        };
        $scope.exportCentralItemData = function () {

            $scope.stores = [];

            $http.get(serviceBase + "api/itemMaster/exportCentral")
                .success(function (data) {
                    $scope.stores = data;
                    alasql('SELECT CategoryName, CategoryCode,SubcategoryName,SubsubcategoryName,BrandCode,itemname,itemcode,Number,SellingSku,price,PurchasePrice,UnitPrice,MinOrderQty,SellingUnitName,PurchaseMinOrderQty,StoringItemName,PurchaseSku,PurchaseUnitName,BaseCategoryName,TGrpName,TotalTaxPercentage,HindiName,SizePerUnit,Barcode,[Active],[Deleted],Margin,PromoPoint,HSNCode INTO XLSX("Items.xlsx",{headers:true}) FROM ?', [$scope.stores]);
                })
                .error(function (data) {
                })

        };

        $scope.exportData = function () {

            alasql('SELECT * INTO XLSX("Items.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };
        $scope.currentPageStores = {};
        $scope.update = function () {

            if ($scope.selectedItem == "TextBox") {
            }
            else if ($scope.selectedItem == "RadioButton") {
            }
            else if ($scope.selectedItem == "MultiSelect") {
            }
        }
        $scope.open = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myitemMasterModal.html",
                    controller: "ModalInstanceCtrladditem", resolve: { itemMaster: function () { return $scope.items } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                    })
        };
        $scope.getWareHouseStores = function () {

            var data = $scope.Warehouseid;
            $scope.pagenoOne = 0;
            $scope.pageno = 1;
            $scope.total_count = 0;
            $scope.getData1($scope.pageno);
        };

        // ************ Start Warehouse Item master operation : Wh = Ware house **************//

        $scope.Whedit = function (item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myWhitemMasterPut.html",
                    controller: "ModalInstanceCtrlWhitemMaster", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };





        $scope.WhSetActive = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myWhactivemodal.html",
                    controller: "ModalInstanceCtrlWhitemMaster", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {

                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };

        $scope.WhSetFree = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myfreemodal.html",
                    controller: "ModalInstanceCtrlWhitemMaster", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {

                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    })
        };

        $scope.Whopendelete = function (data, $index) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModalWhdeleteitemMaster.html",
                    controller: "ModalInstanceCtrlWhdeleteitemMaster", resolve: { itemMaster: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                    })
        };


        $scope.Wholdprice = function (data) {

            $scope.dataoldprice = [];
            var url = serviceBase + "api/itemMaster/Woldprice?ItemId=" + data.ItemId;
            $http.get(url).success(function (response) {
                $scope.dataoldprice = response;
                console.log($scope.dataoldprice);
            })
                .error(function (data) {
                })
        }
        // ************ End Warehouse Item master operation **************//
        //set color for active & inactive
        $scope.set_color = function (data) {

            if (data.active == true) {
                return { background: "lightgreen", color: "black" }
            }
            else if (data.active == false) {
                return { background: "#e94b3b", color: "white" }
            }
            else {
            }
        }
    }
    else {
        window.location = "#/404";
    }
}]);
//******* Start Warehouse item master oparetion controller's **********//
app.controller("ModalInstanceCtrlWhitemMaster", ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemMaster", 'TaxGroupService', 'WarehouseCategoryService', "$filter", function ($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemMaster, TaxGroupService, WarehouseCategoryService, $filter) {

    $scope.xy = true;

    

    $scope.itemMasterData = {};


    $scope.supplier = [];
    supplierService.getsuppliers().then(function (results) {
        $scope.supplier = results.data;
    }, function (error) {
    });
    $scope.Depo = [];
    $scope.GetDepodetails = function (data) {
        supplierService.getdepodata(data).then(function (results) {
            $scope.Depo = results.data;
        }, function (error) {
        });
    };
    // buyer name by anushka(29/08/2019)
    $scope.getBuyer = [];
    $scope.Buyerdata = function () {

        var url = serviceBase + 'api/itemMaster/GetBuyer';
        $http.get(url)
            .success(function (data) {
                $scope.getBuyer = data;
            });
    };
    $scope.Buyerdata();

    if (itemMaster) {

        $scope.itemMasterData = itemMaster;

        $scope.GetDepodetails(itemMaster.SupplierId);
    }
    $scope.ItemPriceData = [];
    $scope.ItemPriceDatatemp = [];
    $scope.GetItemMRP = function (data) {
        var caturl = serviceBase + "api/itemMaster/GetItemMRP?ItemNumber=" + $scope.itemMasterData.Number;
        $http.get(caturl)
            .success(function (data) {
                $scope.ItemPriceData = data;
                $scope.ItemPriceDatatemp = data;
            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            })
    }
    $scope.GetItemMRP();
    //item display name
    $scope.itemname = function (data) {
        $scope.ItemPriceDatatemp = [];
        var id = parseInt(data.ItemMultiMRPId);
        $scope.filterData = $filter('filter')($scope.ItemPriceData, function (value) {
            return value.ItemMultiMRPId === id;
        });
        $scope.ItemPriceDatatemp = $scope.filterData;
        data.price = $scope.filterData[0].MRP;
        $scope.itemMasterData.itemname = '';
        $scope.itemMasterData.SellingUnitName = '';
        alert("Please Change Purchase Price and Unit Price!!");
        if (data.itemBaseName == undefined) {
            data.itemBaseName = '';
        }
        if (data.price == undefined) { data.price = ''; }
        if (data.UnitofQuantity == undefined) { data.UnitofQuantity = ''; }
        if (data.UOM == undefined) { data.UOM = ''; }
        //$scope.itemMasterData.itemname = data.itemBaseName + " " + $scope.filterData[0].MRP + " MRP " + $scope.filterData[0].UnitofQuantity + " " + $scope.filterData[0].UOM;

        // by anushka(29/04/2019)
        if (data.IsSensitive == 'true') {
            $scope.itemMasterData.itemname = data.itemBaseName + " " + $scope.filterData[0].MRP + " MRP " + $scope.filterData[0].UnitofQuantity + " " + $scope.filterData[0].UOM;

        }
        else {
            $scope.itemMasterData.itemname = data.itemBaseName + " " + $scope.filterData[0].MRP + " MRP ";
        }

        //item purchase name
        if (data.PurchaseMinOrderQty > 0) {


            $scope.itemMasterData.UnitofQuantity = $scope.filterData[0].UnitofQuantity;
            $scope.itemMasterData.PurchaseUnitName = $scope.itemMasterData.itemname + " " + data.PurchaseMinOrderQty + "Unit";
        }
        //item selling unit name
        if (data.MinOrderQty > 0) {

            $scope.itemMasterData.UOM = $scope.filterData[0].UOM;
            $scope.itemMasterData.SellingUnitName = $scope.itemMasterData.itemname + " " + data.MinOrderQty + "Unit";
        }
    }

    //item purchase name
    $scope.purchaseUnitname = function (data) {

        if (data.PurchaseMinOrderQty > 0) {
            $scope.itemMasterData.PurchaseUnitName = $scope.itemMasterData.itemname + " " + data.PurchaseMinOrderQty + "Unit";
        }
    };

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.PutitemMaster = function (data) {

        if (data.UnitPrice == 0 || data.PurchasePrice == 0) { alert("Insert UnitPrice & PurchasePrice"); return false; }
        if (!data.SupplierId) { alert(" Please select supplier "); return false;}

        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
                
                var url = serviceBase + "api/itemMaster";

                var dataToPost = {
                    WareHouseItemId: data.WareHouseItemId,
                    ItemId: $scope.itemMasterData.ItemId,
                    itemname: data.itemname,
                    itemBaseName: data.itemBaseName,
                    itemcode: data.itemcode,
                    Cityid: data.Cityid,
                    CityName: data.CityName,
                    BaseCategoryid: data.BaseCategoryid,
                    Categoryid: data.Categoryid,
                    CategoryName: data.CategoryName,
                    SubCategoryId: data.SubCategoryId,
                    SubcategoryName: data.SubcategoryName,
                    SubsubCategoryid: data.SubsubCategoryid,
                    SellingSku: data.SellingSku,
                    GruopID: data.GruopID,
                    CessGrpID: data.CessGrpID,
                    SupplierId: $scope.itemMasterData.SupplierId,
                    MinOrderQty: $scope.itemMasterData.MinOrderQty,
                    PurchaseMinOrderQty: $scope.itemMasterData.PurchaseMinOrderQty,
                    UnitId: $scope.itemMasterData.UnitId,
                    Id: $scope.itemMasterData.Id,
                    UnitPrice: $scope.itemMasterData.UnitPrice,
                    Discount: $scope.itemMasterData.Discount,
                    VATTax: data.VATTax,
                    GeneralPrice: $scope.itemMasterData.GeneralPrice,
                    price: $scope.itemMasterData.price,
                    PurchaseUnitName: data.PurchaseUnitName,
                    SellingUnitName: data.SellingUnitName,
                    PurchasePrice: $scope.itemMasterData.PurchasePrice,
                    SellingPrice: $scope.itemMasterData.SellingPrice,
                    UpdatedDate: $scope.itemMasterData.UpdatedDate,
                    active: $scope.itemMasterData.active,
                    Margin: $scope.itemMasterData.Margin,
                    WarehouseId: $scope.itemMasterData.WarehouseId,
                    promoPerItems: $scope.itemMasterData.promoPerItems,
                    free: $scope.itemMasterData.free,
                    Placeorder: data.Placeorder,
                    HindiName: data.HindiName,
                    promoPoint: data.promoPoint,
                    IsDailyEssential: data.IsDailyEssential,
                    HSNCode: $scope.itemMasterData.HSNCode,
                    UnitofQuantity: data.UnitofQuantity,
                    ItemMultiMRPId: data.ItemMultiMRPId,
                    IsSensitive: data.IsSensitive,
                    DepoId: data.DepoId,
                    DepoName: data.DepoName,
                    UOM: data.UOM,
                    MRP: data.price,
                    BuyerId: data.BuyerId,
                    BuyerName: data.BuyerName
                };
                $http.put(url, dataToPost)
                    .success(function (data) {
                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            alert('Updated Successfully');
                          
                            $modalInstance.close(data);
                        }
                    })
                    .error(function (data) {
                    })

        };

}]);


app.controller("ModalInstanceCtrlWhdeleteitemMaster", ["$scope", '$http', "$modalInstance", "itemMasterService", 'ngAuthSettings', "itemMaster", function ($scope, $http, $modalInstance, itemMasterService, ngAuthSettings, itemMaster) {
    //User Tracking
    //$scope.AddTrack = function (Atype, page, Detail) {

    //    console.log("Tracking Code");
    //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
    //    $http.post(url).success(function (results) { });
    //}
    //End User Tracking
    $scope.warehouse = [];
    function ReloadPage() {
        location.reload();
    };
    if (itemMaster) {
        $scope.itemMasterData = itemMaster;
    }
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


        $scope.deleteitemMaster = function (dataToPost, $index) {

            $http.delete(serviceBase + 'api/itemMaster/warehouseitem?id=' + dataToPost.ItemId).then(function (results) {
                return results;
                $modalInstance.close();
                ReloadPage();
            });

        }
}])
//******* End Warehouse item master oparetion controller's **********//


//******* Start Central item master oparetion controller's **********//

app.controller("ModalInstanceCtrlitemMaster", ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemMaster", 'TaxGroupService', 'WarehouseCategoryService', function ($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemMaster, TaxGroupService, WarehouseCategoryService) {
    //User Tracking

    //$scope.AddTrack = function (Atype, page, Detail) {

    //    console.log("Tracking Code");
    //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
    //    $http.post(url).success(function (results) { });
    //}
    //End User Tracking
    $scope.xy = true;
    $scope.upld = true;
    $scope.validateUpload = function () {

        $scope.upld = true;
    }
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
                var fileuploadurl = '/api/itemimageupload/post', files;
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

    $scope.itemMasterData = {};

    $scope.city = [];
    CityService.getcitys().then(function (results) {
        $scope.city = results.data;
    }, function (error) {
    });



    $scope.subcategory = [];
    SubCategoryService.getsubcategorys().then(function (results) {
        $scope.subcategory = results.data;
    }, function (error) {
    });

    $scope.subsubcategory = [];
    SubsubCategoryService.getsubsubcats().then(function (results) {     
        $scope.subsubcategoryedit = results.data;
    }, function (error) {
    });

    $scope.category = [];
    CategoryService.getcategorys().then(function (results) {
        $scope.category = results.data;
    }, function (error) {
    });

    $scope.taxgroups = [];
    TaxGroupService.getTaxGroup().then(function (results) {
        $scope.taxgroups = results.data;
    }, function (error) {
    });


    if (itemMaster) {

        $scope.itemMasterData = itemMaster;
    }


    $scope.ItemPriceData = [];
    $scope.GetItemMRP = function (data) {
        var caturl = serviceBase + "api/itemMaster/GetItemMRP?ItemNumber=" + $scope.itemMasterData.Number;
        $http.get(caturl).success(function (data) {
            $scope.ItemPriceData = data;
        })
            .error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            })
    }
    $scope.GetItemMRP();



    //item display name
    $scope.itemname = function (data) {

        $scope.itemMasterData.itemname = '';
        $scope.itemMasterData.SellingUnitName = '';
        if (data.itemBaseName == undefined) {
            data.itemBaseName = '';
        }
        if (data.price == undefined) { data.price = ''; }
        if (data.UnitofQuantity == undefined) { data.UnitofQuantity = ''; }
        if (data.UOM == undefined) { data.UOM = ''; }
        //$scope.itemMasterData.itemname = data.itemBaseName + " " + data.price + " MRP " + data.UnitofQuantity + " " + data.UOM;

        // by anushka(29/04/2019)
        if (data.IsSensitive.toString() == true || data.IsSensitive.toString() == "true") {

            if (data.IsSensitiveMRP.toString() == 'false') {
                $scope.itemMasterData.itemname = data.itemBaseName + " " + data.UnitofQuantity + " " + data.UOM;

            } else {
                $scope.itemMasterData.itemname = data.itemBaseName + " " + data.price + " MRP " + data.UnitofQuantity + " " + data.UOM;
            }
        }
        else {
            $scope.itemMasterData.itemname = data.itemBaseName + " " + data.price + " MRP ";
        }
        //if (data.IsSensitive.toString() == 'true') {
        //    $scope.itemMasterData.itemname = data.itemBaseName + " " + data.price + " MRP " + data.UnitofQuantity + " " + data.UOM;

        //}
        //else {
        //    $scope.itemMasterData.itemname = data.itemBaseName + " " + data.price + " MRP ";
        //}


        $scope.itemMasterData.SellingUnitName = $scope.itemMasterData.itemname;

        $scope.purchaseUnitname($scope.itemMasterData);

        $scope.SellingUnitname($scope.itemMasterData);
    }

    //item purchase name
    $scope.purchaseUnitname = function (data) {

        if (data.PurchaseMinOrderQty > 0) {
            $scope.itemMasterData.PurchaseUnitName = $scope.itemMasterData.itemname + " " + data.PurchaseMinOrderQty + "Unit";

        }
    }


    //item selling unit name
    $scope.SellingUnitname = function (data) {

        if (data.MinOrderQty > 0) {
            $scope.itemMasterData.SellingUnitName = $scope.itemMasterData.itemname + " " + data.MinOrderQty + "Unit";
        }

    }


    $scope.ok = function () { $modalInstance.close(); };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    $scope.PutitemMaster = function (data) {
        
        if ($scope.itemMasterData.ShowTypes == null || $scope.itemMasterData.ShowTypes == "" || $scope.itemMasterData.ShowTypes == undefined) {
            alert("Please select item Show Types:");
            return false;
        }
        $scope.loogourl = itemMaster.LogoUrl;
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

            console.log($scope.uploadedfileName);
        if ($scope.uploadedfileName != "") {
            $scope.loogourl = $scope.uploadedfileName;
        };

        // if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {
        var url = serviceBase + "api/itemMaster/PutCItem";

        var dataToPost = {
            Id: $scope.itemMasterData.Id,
            ItemId: $scope.itemMasterData.Id,
            itemname: $scope.itemMasterData.itemname,
            itemcode: $scope.itemMasterData.itemcode,
            Cityid: $scope.itemMasterData.Cityid,
            Categoryid: $scope.itemMasterData.Categoryid,
            SubCategoryId: $scope.itemMasterData.SubCategoryId,
            SubsubCategoryid: $scope.itemMasterData.SubsubCategoryid,
            SupplierId: $scope.itemMasterData.SupplierId,
            MinOrderQty: $scope.itemMasterData.MinOrderQty,
            PurchaseMinOrderQty: $scope.itemMasterData.PurchaseMinOrderQty,
            UnitId: $scope.itemMasterData.UnitId,           
            UnitPrice: $scope.itemMasterData.UnitPrice,
            Discount: $scope.itemMasterData.Discount,
            GruopID: $scope.itemMasterData.GruopID,
            CessGrpID: $scope.itemMasterData.CessGrpID,
            Placeorder: $scope.itemMasterData.Placeorder,
            GeneralPrice: $scope.itemMasterData.GeneralPrice,
            Number: $scope.itemMasterData.Number,
            Barcode: $scope.itemMasterData.Barcode,
            PurchaseUnitName: $scope.itemMasterData.PurchaseUnitName,
            SellingUnitName: $scope.itemMasterData.SellingUnitName,
            price: $scope.itemMasterData.price,
            PurchaseSku: $scope.itemMasterData.PurchaseSku,
            PurchasePrice: $scope.itemMasterData.PurchasePrice,
            SellingSku: $scope.itemMasterData.SellingSku,
            SellingPrice: $scope.itemMasterData.UnitPrice,
            VATTax: $scope.itemMasterData.VATTax,
            UpdatedDate: $scope.itemMasterData.UpdatedDate,
            LogoUrl: $scope.loogourl,
            active: $scope.itemMasterData.active,
            IsDailyEssential: $scope.itemMasterData.IsDailyEssential,
            Margin: $scope.itemMasterData.Margin,
            promoPoint: $scope.itemMasterData.promoPoint,
            HindiName: $scope.itemMasterData.HindiName,
            WarehouseId: $scope.itemMasterData.WarehouseId,
            promoPerItems: $scope.itemMasterData.promoPerItems,
            free: $scope.itemMasterData.free,
            HSNCode: $scope.itemMasterData.HSNCode,
            ShowTypes: $scope.itemMasterData.ShowTypes,
            ShowMrp: $scope.itemMasterData.ShowMrp,
            ShowUnit: $scope.itemMasterData.ShowUnit,
            UOM: $scope.itemMasterData.UOM,
            ShowUOM: $scope.itemMasterData.ShowUOM,
            ShowType: $scope.itemMasterData.ShowType,
            Reason: $scope.itemMasterData.Reason,
            DefaultBaseMargin: $scope.itemMasterData.DefaultBaseMargin,
            itemBaseName: $scope.itemMasterData.itemBaseName,
            UnitofQuantity: $scope.itemMasterData.UnitofQuantity,
            ItemMultiMRPId: $scope.itemMasterData.ItemMultiMRPId,
            IsSensitive: $scope.itemMasterData.IsSensitive,
            IsSensitiveMRP: $scope.itemMasterData.IsSensitiveMRP

        };

        $http.put(url, dataToPost)
            .success(function (data) {
                if (data.id == 0) {
                    $scope.gotErrors = true;
                    if (data[0].exception == "Already") {
                        $scope.AlreadyExist = true;
                    }
                }
                else {
                    alert('Updated Successfully');
                    //$scope.AddTrack("Edit(Item)", "ItemName:", dataToPost.itemname);
                    $modalInstance.close(data);
                }
            })
            .error(function (data) {
            })
        //}

        //else {


        //}
    };
    $scope.sa = itemMaster.Number;
    var uploader = $scope.uploader = new FileUploader({
        url: ""        //url: serviceBase + 'api/itemUpload/?type=' + $scope.itemMasterData.itemcode
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
        console.info('onAfterAddingFile', fileItem);
        var fileExtension = '.' + fileItem.file.name.split('.').pop();
        fileItem.file.name = $scope.sa;
    };
    uploader.onAfterAddingAll = function (addedFileItems) {
        if ($scope.itemMasterData.itemcode == null) {
            alert("Enter item code");
            $scope.upld = false;
        } else {
            $scope.upld = true;
        }
    };
    uploader.onBeforeUploadItem = function (item) {
        item.url = serviceBase + 'api/itemimageupload/?type=' + $scope.itemMasterData.itemcode;
    };
    uploader.onProgressItem = function (fileItem, progress) { };
    uploader.onProgressAll = function (progress) { };
    uploader.onSuccessItem = function (fileItem, response, status, headers) {


    };
    uploader.onErrorItem = function (fileItem, response, status, headers) { };
    uploader.onCancelItem = function (fileItem, response, status, headers) { };
    uploader.onCompleteItem = function (fileItem, response, status, headers) {

        $scope.uploadedfileName = fileItem._file.name;
        response = response.slice(1, -1);
        $scope.fileurl = response;
        $scope.uploadedfileName = response;
    };
    uploader.onCompleteAll = function () { };
}]);

app.controller("ModalInstanceCtrladditem", ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemMaster", 'TaxGroupService', 'WarehouseCategoryService', function ($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemMaster, TaxGroupService, WarehouseCategoryService) {
    //User Tracking
    $scope.TempIschild = false;
    $scope.xy = '1';
    $scope.datasTa = [];
    itemMasterService.CentralGetitemMaster().then(function (results) {
        ;
        $scope.datasTa = results.data.ItemMasterCentral;
        if ($scope.datasTa) {
            $scope.GetUniqueitemMaster(results.data.ItemMasterCentralGroupByNumber);
        }
    }, function (error) {
    });

    $scope.datas = [];
    $scope.GetUniqueitemMaster = function (itemlist) {

        $scope.datas = itemlist;
        angular.forEach($scope.datas, function (obj) {
            obj.itemBaseName = obj.itemBaseName + ":  (" + obj.itemname + ")";
        });

    };

    $scope.city = [];
    CityService.getcitys().then(function (results) {

        $scope.city = results.data;

    }, function (error) {
    });

    $scope.upld = false;
    $scope.validateUpload = function () {
        if ($scope.itemMasterData.itemcode != null) {
            $scope.upld = true;
        } else {
            $scope.upld = false;
        }
    }
    $scope.validateUploadd = function () {

        if ($scope.itemMasterData.SellingSku != null) {
            $scope.upld = true;
        } else {
            $scope.upld = false;
        }
    }

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
                var fileuploadurl = '/api/itemimageupload/post', files;
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
    $scope.itemMasterData = {};

    $scope.warehouse = [];
    WarehouseService.getwarehouse().then(function (results) {

        $scope.warehouse = results.data;

    }, function (error) {
    });



    $scope.subsubcategory = [];
    SubsubCategoryService.getsubsubcats().then(function (results) {
        $scope.subsubcategory = results.data;
    }, function (error) {
    });

    $scope.subcategory = [];
    SubCategoryService.getsubcategorys().then(function (results) {

        $scope.subcategory = results.data;
    }, function (error) {
    });

    $scope.category = [];
    CategoryService.getcategorys().then(function (results) {
        $scope.category = results.data;
    }, function (error) {
    });

    $scope.taxgroups = [];
    TaxGroupService.getTaxGroup().then(function (results) {
        $scope.taxgroups = results.data;

    }, function (error) {
    });



    $scope.GetSubCategory = function (data) {
        $scope.subcategorydata = [];
        angular.forEach($scope.subcategory, function (value, key) {
            if (value.Categoryid == data.Categoryid) {
                $scope.subcategorydata.push(value);
            }
            else {
            }
            console.log($scope.subcategorydata);
        });
    }

    $scope.GetSubSubCategory = function (data) {
        $scope.subsubcategorydata = [];

        angular.forEach($scope.subsubcategory, function (value, key) {
            if (value.SubCategoryId == data.SubCategoryId) {
                $scope.subsubcategorydata.push(value);
            }
            else {
            }
            console.log($scope.subsubcategorydata);
        });
    }

    $scope.supplier = [];
    supplierService.getsuppliers().then(function (results) {
        $scope.supplier = results.data;
    }, function (error) {
    });

    var icode = $scope.itemMasterData.itemcode;
    if (itemMaster) {
        $scope.itemMasterData = itemMaster;
    }
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        //for auto selection
        $scope.selectedData = null;

    $scope.onSelect = function (selection) {
        $scope.TempIschild = true;
        $scope.sdt = false;
        $scope.SKUMutiHide = [];
        console.log(selection);
        $scope.selectedData = selection;
        $scope.itemMasterData = selection;

        $scope.subsubcategorydata = [];
        angular.forEach($scope.subsubcategory, function (value, key) {
            if (value.SubCategoryId == selection.SubCategoryId) {
                $scope.subsubcategorydata.push(value);
            }
            else {
            }
            console.log($scope.subsubcategorydata);
        });


        $scope.subcategorydata = [];
        angular.forEach($scope.subcategory, function (value, key) {
            if (value.Categoryid == selection.Categoryid) {
                $scope.subcategorydata.push(value);
            }
            else {
            }
            console.log($scope.subcategorydata);
        });

        $scope.xy = '0';
        angular.forEach($scope.datasTa, function (value, key) {
            if (value.Number == $scope.selectedData.Number) {

                $scope.itemMasterData.itemBaseName = value.itemBaseName;
                $scope.SKUMutiHide.push(value.SellingSku);
            }
            console.log($scope.SKUMutiHide);
        });
        $scope.xy = '0';
        $scope.getWsubsubcat($scope.selectedData);
    };


    //For Item Code
    $scope.getItemCode = function (itemData) {

        $http.get(serviceBase + 'api/itemMaster/GenerateItemCode').then(function (results) {
            var catCode = "";
            var brandCode = "";

            $scope.itemMasterData.itemcode = parseInt(results.data);
            $scope.xy = '0';
            angular.forEach($scope.category, function (value, key) {
                if (value.Categoryid == itemData.Categoryid)
                    catCode = value.Code;
            });
            angular.forEach($scope.subsubcategorydata, function (value, key) {
                if (value.SubsubCategoryid == itemData.SubsubCategoryid)
                    brandCode = value.Code;
            });
            if (catCode != "" && brandCode != "") {

                $scope.itemMasterData.Number = catCode + brandCode + results.data;
                $scope.itemMasterData.PurchaseSku = $scope.itemMasterData.Number + "P";

            } else {
                alert('category code can not be null')
            }

        }, function (error) {
        });
    };

    //item display name
    $scope.itemname = function (data) {
        
        $scope.itemMasterData.itemname = '';
        $scope.itemMasterData.SellingUnitName = '';
        if (data.itemBaseName == undefined) {
            data.itemBaseName = '';
        }
        if (data.price == undefined) { data.price = ''; }
        if (data.UnitofQuantity == undefined) { data.UnitofQuantity = ''; }
        if (data.UOM == undefined) { data.UOM = ''; }
        // $scope.itemMasterData.itemname = data.itemBaseName + " " + data.price + " MRP " + data.UnitofQuantity + " " + data.UOM;
        //Anu(29/04/2019)
        if (data.IsSensitive.toString() == true || data.IsSensitive.toString() == "true") {

            if (data.IsSensitiveMRP == 'false') {
                $scope.itemMasterData.itemname = data.itemBaseName + " " + data.UnitofQuantity + " " + data.UOM;

            } else {
                $scope.itemMasterData.itemname = data.itemBaseName + " " + data.price + " MRP " + data.UnitofQuantity + " " + data.UOM;
            }
        }
        else {
            $scope.itemMasterData.itemname = data.itemBaseName + " " + data.price + " MRP ";
        }

        $scope.itemMasterData.SellingUnitName = $scope.itemMasterData.itemname;
        $scope.itemMasterData.PurchaseUnitName = $scope.itemMasterData.itemname;
    };

    //item purchase name
    $scope.purchaseUnitname = function (data) {

        if (data.PurchaseMinOrderQty > 0) {
            $scope.itemMasterData.PurchaseUnitName = $scope.itemMasterData.itemname + " " + data.PurchaseMinOrderQty + "Unit";
        }

    };

    //item selling unit name
    $scope.SellingUnitname = function (data) {

        if (data.MinOrderQty > 0) {
            $scope.itemMasterData.SellingUnitName = $scope.itemMasterData.itemname + " " + data.MinOrderQty + "Unit";
        }

    };

    $scope.AdditemMaster = function (data) {

        if (data.itemname == null || data.itemname == "") {
            alert('Please Enter Base Name');
            $modalInstance.open();
        }
        else if (data.HSNCode == null || data.HSNCode == "") {
            alert('Please Enter HSNC Code');
            $modalInstance.open();
        }
        else if (data.price == null || data.price == "") {
            alert('Please Enter MRP');
            $modalInstance.open();
        }
        else if (data.MinOrderQty == null || data.MinOrderQty == "") {
            alert('Please Enter Minimum Order Qty');
            $modalInstance.open();
        }
        else if (data.PurchaseMinOrderQty == null || data.PurchaseMinOrderQty == "") {
            alert('Please Enter Minimum Purchase Qty');
            $modalInstance.open();
        }
        else if (data.GruopID == "" || data.GruopID == undefined) {
            alert('Please Enter Tax Group');
            $modalInstance.open();
        }
        else if (data.ShowTypes == null || data.ShowTypes == "") {
            alert('Please Enter ShowTypes');
            $modalInstance.open();
        }

        var LogoUrl = $scope.uploadedfileName;
        if (LogoUrl !== "") {
            $scope.itemMasterData.LogoUrl = LogoUrl;
        }
        var url = serviceBase + "api/itemMaster";
        if ($scope.itemMasterData.Barcode == null || $scope.itemMasterData.Barcode == "") {
            $scope.itemMasterData.Barcode == $scope.itemMasterData.Number;
        }
        if ($scope.itemMasterData.ShowTypes == null || $scope.itemMasterData.ShowTypes == "" || $scope.itemMasterData.ShowTypes == undefined) {
            alert("Please select item Show Types:");
            return false;
        }

        if ($scope.itemMasterData.itemname != null && $scope.itemMasterData.itemname != "") {
            if ($scope.itemMasterData.Number != null && $scope.itemMasterData.Number != "") {
                if ($scope.itemMasterData.SellingSku != null && $scope.itemMasterData.SellingSku != "") {
                    if ($scope.itemMasterData.PurchaseSku != null && $scope.itemMasterData.PurchaseSku != "") {
                        if ($scope.itemMasterData.SubsubCategoryid != null && $scope.itemMasterData.SubsubCategoryid != "") {

                            var dataToPost = {
                                itemname: $scope.itemMasterData.itemname,
                                Categoryid: $scope.itemMasterData.Categoryid,
                                SubCategoryId: $scope.itemMasterData.SubCategoryId,
                                SubsubCategoryid: $scope.itemMasterData.SubsubCategoryid,
                                Cityid: $scope.itemMasterData.Cityid,
                                SupplierId: $scope.itemMasterData.SupplierId,
                                itemcode: $scope.itemMasterData.itemcode,
                                GeneralPrice: $scope.itemMasterData.GeneralPrice,
                                Number: $scope.itemMasterData.Number,
                                Barcode: $scope.itemMasterData.Barcode,
                                PurchaseUnitName: $scope.itemMasterData.PurchaseUnitName,
                                SellingUnitName: $scope.itemMasterData.SellingUnitName,
                                UnitPrice: $scope.itemMasterData.UnitPrice,
                                Discount: $scope.itemMasterData.Discount,
                                GruopID: $scope.itemMasterData.GruopID,
                                CessGrpID: $scope.itemMasterData.CessGrpID,
                                MinOrderQty: $scope.itemMasterData.MinOrderQty,
                                PurchaseMinOrderQty: $scope.itemMasterData.PurchaseMinOrderQty,
                                price: $scope.itemMasterData.price,
                                WarehouseId: $scope.itemMasterData.WarehouseId,
                                PurchaseSku: $scope.itemMasterData.PurchaseSku,
                                PurchasePrice: $scope.itemMasterData.PurchasePrice,
                                SellingSku: $scope.itemMasterData.SellingSku,
                                SellingPrice: $scope.itemMasterData.UnitPrice,
                                LogoUrl: $scope.itemMasterData.LogoUrl,
                                active: true,
                                Margin: $scope.itemMasterData.Margin,
                                promoPoint: $scope.itemMasterData.promoPoint,
                                HindiName: $scope.itemMasterData.HindiName,
                                HSNCode: $scope.itemMasterData.HSNCode,
                                IsDailyEssential: $scope.itemMasterData.IsDailyEssential,
                                ShowMrp: $scope.itemMasterData.ShowMrp,
                                ShowUnit: $scope.itemMasterData.ShowUnit,
                                ShowUOM: $scope.itemMasterData.ShowUOM,
                                ShowType: $scope.itemMasterData.ShowType,
                                ShowTypes: $scope.itemMasterData.ShowTypes,
                                Reason: $scope.itemMasterData.Reason,
                                DefaultBaseMargin: $scope.itemMasterData.DefaultBaseMargin,
                                itemBaseName: $scope.itemMasterData.itemBaseName,
                                UnitofQuantity: $scope.itemMasterData.UnitofQuantity,
                                ItemMultiMRPId: $scope.itemMasterData.ItemMultiMRPId,
                                UOM: $scope.itemMasterData.UOM,
                                MRP: $scope.itemMasterData.price,
                                IsSensitive: $scope.itemMasterData.IsSensitive,
                                IsSensitiveMRP: $scope.itemMasterData.IsSensitiveMRP
                            };

                            $http.post(url, dataToPost)
                                .success(function (data) {
                                    if (data.ItemId == 0) {
                                        alert("Item Not Saved May be Selling SKU exist already");
                                        $scope.gotErrors = true;
                                        if (data[0].exception == "Already") {
                                            $scope.AlreadyExist = true;
                                        }
                                    }
                                    else {
                                        $modalInstance.close(data);
                                        alert("Item created Successfully");
                                        //$scope.AddTrack("Add(NewItem)", "ItemNAme:", dataToPost.itemname);

                                    }
                                })
                                .error(function (data) {
                                    alert("Item Not Saved May be Selling SKU exist already");
                                })
                        }
                        else {
                            alert("SubSubCategory must be filled out");
                        }
                    }
                    else {
                        alert("PurchaseSku must be filled out");
                    }
                }
                else {
                    alert("SellingSku must be filled out");
                }
            }
            else {
                alert("Item Number must be filled out");
            }
        }
        else {
            alert("Item must be filled out");
        }
    };
    /////////////////////////////////////////////////////// angular upload code    
    var uploader = $scope.uploader = new FileUploader({
        url: ""
        //url: serviceBase + 'api/itemUpload/?type=' + $scope.itemMasterData.itemcode
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
    //Code Added By Previn
    uploader.onAfterAddingFile = function (fileItem) {

        if ($scope.itemMasterData.Number == null) {
            alert("Enter item number");
        }
        else {
            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = $scope.itemMasterData.Number;
        }

    };
    uploader.onAfterAddingAll = function (addedFileItems) {
        if ($scope.itemMasterData.itemcode == null) {
            alert("Enter item code");
            $scope.upld = false;
        } else {
            $scope.upld = true;

        }
    };
    uploader.onBeforeUploadItem = function (item) {

        item.url = serviceBase + 'api/itemUpload/?type=' + $scope.itemMasterData.itemcode;

    };
    uploader.onProgressItem = function (fileItem, progress) {
    };
    uploader.onProgressAll = function (progress) {
    };
    uploader.onSuccessItem = function (fileItem, response, status, headers) {
    };
    uploader.onErrorItem = function (fileItem, response, status, headers) {
    };
    uploader.onCancelItem = function (fileItem, response, status, headers) {
    };
    uploader.onCompleteItem = function (fileItem, response, status, headers) {

        //$scope.uploadedfileName = fileItem._file.name;old
        // below aded by previn
        response = response.slice(1, -1);
        $scope.LogoUrl = response;
        $scope.uploadedfileName = response;
    };
    uploader.onCompleteAll = function () {

    };
}]);

app.controller("ModalInstanceCtrldeleteitemMaster", ["$scope", '$http', "$modalInstance", "itemMasterService", 'ngAuthSettings', "itemMaster", function ($scope, $http, $modalInstance, itemMasterService, ngAuthSettings, itemMaster) {
    //User Tracking
    //$scope.AddTrack = function (Atype, page, Detail) {

    //    console.log("Tracking Code");
    //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
    //    $http.post(url).success(function (results) { });
    //}

    //End User Tracking
    $scope.warehouse = [];
    function ReloadPage() {
        location.reload();
    };
    if (itemMaster) {
        $scope.itemMasterData = itemMaster;


    }
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


        $scope.deleteitemMaster = function (dataToPost, $index) {

            itemMasterService.deleteitemMaster(dataToPost).then(function (results) {

                //$scope.AddTrack("Delete(Item)", "ItemName:", dataToPost.itemname);

                $modalInstance.close(dataToPost);
                //ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        };

}]);

//******* End Central item master oparetion controller's **********//

//MultiMrp Harry 30/03/2019
app.controller("ModalInstanceCtrladdMultiplePrice", ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemMaster", 'TaxGroupService', 'WarehouseCategoryService', function ($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemMaster, TaxGroupService, WarehouseCategoryService) {

    $scope.MrpUserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.itemMasterData = {};
    if (itemMaster) {
        $scope.itemMasterData = itemMaster;
    }
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

    var ItemDataPrice = {};//temp 

    $scope.ItemFinalData = [];

    $scope.ItemPriceData = {};

    $scope.Add = function (MRPData) {

        if (MRPData.UnitofQuantity == 0 || MRPData.UnitofQuantity == undefined) {
            alert('insert UnitofQuantity');
            return false;
        }
        if (MRPData.UOM == 0 || MRPData.UOM == undefined) {
            alert('select UOM');
            return false;
        }
        if (MRPData.Price == 0 || MRPData.Price == undefined) {
            alert('insert mrp');
            return false;
        }
        else {
            $scope.xy = '0';
            ItemDataPrice.MRP = MRPData.Price;
            ItemDataPrice.ItemNumber = $scope.itemMasterData.Number;
            ItemDataPrice.UnitofQuantity = MRPData.UnitofQuantity;
            ItemDataPrice.UOM = MRPData.UOM;
            ItemDataPrice.itemname = $scope.itemMasterData.itemname;
            ItemDataPrice.itemBaseName = $scope.itemMasterData.itemBaseName;
            $scope.ItemFinalData.push(ItemDataPrice);
            $scope.ItemPriceData.MRP = "";
        }
    };

    $scope.AdditemMRP = function (data) {

        if ($scope.ItemFinalData.length == 0) {
            alert("Insert New MRP");
            return false;
        }

        if (data.Id == "" || data.Id == undefined) {
            alert('Item Name is Required');
            return false;
        }
        else {
            var url = serviceBase + "api/itemMaster/AddItemMRP";
            var dataToPost = $scope.ItemFinalData;
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (data) {

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
                });
        }
    };

    //get list of mrp of item  using Item id

    $scope.GetItemMRP = function (data) {

        var caturl = serviceBase + "api/itemMaster/GetItemMRP?ItemNumber=" + $scope.itemMasterData.Number;
        $http.get(caturl)
            .success(function (data) {

                $scope.GetItemMRPData = data;
            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            });
    };
    $scope.GetItemMRP();

    //09/04/19 EditCode For MultiMrp by Ashwin
    $scope.editData = function (record) {

        angular.extend($scope.recordCache = record, { editing: true });
    };
    //update multi mrp code
    $scope.mrpedit = function (data) {

        var url = serviceBase + "api/itemMaster/PutItemMRP";
        var itemMultiMRP = {
            ItemMultiMRPId: data.ItemMultiMRPId,
            MRP: data.MRP,
            UnitofQuantity: data.UnitofQuantity,
            UOM: data.UOM,
            itemBaseName: $scope.itemMasterData.itemBaseName

        };
        $http.put(url, itemMultiMRP)
            .success(function (data) {

                if (data.id == 0) {

                }
                else {
                    alert("record updated successfully");
                    $modalInstance.close(data);
                }
            })
            .error(function (data) {
            });
    };
    //09/04/19 EditCode For MultiMrp by Ashwin//end

}]);

//ItemLimit Anushka 05 /06/2019
app.controller("ModalInstanceCtrlWhAddItemLimit", ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemLimit", 'TaxGroupService', 'WarehouseCategoryService', function ($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemLimit, TaxGroupService, WarehouseCategoryService) {
    $scope.AddLimitUserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.ItemAddLimitData = {};
    if (itemLimit) {
        $scope.ItemAddLimitData = itemLimit;
    }
    $scope.GetLimit = function (data) {
        var url = serviceBase + "api/itemMaster/GetItemLimit?itemNumber=" + $scope.ItemAddLimitData.Number + "&WarehouseId=" + $scope.ItemAddLimitData.WarehouseId + "&multiMrpId=" + $scope.ItemAddLimitData.ItemMultiMRPId;
        $http.get(url)
            .success(function (response) {
                if (response != null) {
                    $scope.ItemAddLimitData.ItemlimitQty = response.ItemlimitQty ? response.ItemlimitQty : 0;
                    $scope.ItemAddLimitData.IsItemLimit = response.IsItemLimit;
                }
                else {
                    $scope.ItemAddLimitData.ItemlimitQty = 0;
                    $scope.ItemAddLimitData.IsItemLimit = false;
                }

            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                console.log(data);
            });
    };
    $scope.GetLimit();

    $scope.putLimit = function (ItemAddLimitData) {

        if ($scope.ItemAddLimitData.ItemlimitQty >= 0) {
            var url = serviceBase + "api/itemMaster/PutItemLimit";
            var dataToPost = {
                ItemId: $scope.ItemAddLimitData.ItemId,
                WarehouseId: $scope.ItemAddLimitData.WarehouseId,
                ItemNumber: $scope.ItemAddLimitData.Number,
                ItemlimitQty: $scope.ItemAddLimitData.ItemlimitQty,
                ItemLimitSaleQty: $scope.ItemAddLimitData.ItemLimitSaleQty,
                IsItemLimit: $scope.ItemAddLimitData.IsItemLimit,
                ItemMultiMRPId: $scope.ItemAddLimitData.ItemMultiMRPId,
                Id: $scope.ItemAddLimitData.ItemLimitId
            };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {

                })


                .error(function (data) {
                    console.log(data);
                });
            $scope.isDisable = true;
            $modalInstance.close();
        }
        else {

            alert('please enter plus Qty');
        }
    };

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
}]);


//upload image Pooja 10/07/2019
app.controller("ModalInstanceCtrladdItemImage", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'FileUploader', "itemMaster", function ($scope, $http, ngAuthSettings, $modalInstance, FileUploader, itemMaster) {

    $scope.ImageUserRole = JSON.parse(localStorage.getItem('RolePerson'));

    $scope.itemMasterData = {};
    if (itemMaster) {
        $scope.itemMasterData = itemMaster;
    }
    $scope.uploadedfileName = '';
    //$scope.cancel = function () { $modalInstance.dismiss('canceled'); },

    //Itemimage Uploader
    $scope.Number = $scope.itemMasterData.Number;
    var uploader = $scope.uploader = new FileUploader({
        url: ""
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
        console.info('onAfterAddingFile', fileItem);
        var fileExtension = '.' + fileItem.file.name.split('.').pop();
        fileItem.file.name = $scope.Number;
    };
    uploader.onAfterAddingAll = function (addedFileItems) {

        if ($scope.itemMasterData.itemcode == null) {
            alert("Enter item code");
            $scope.upld = false;
        } else {
            $scope.upld = true;
        }
    };
    uploader.onBeforeUploadItem = function (item) {

        item.url = serviceBase + 'api/itemimageupload/?type=' + $scope.itemMasterData.itemcode;
    };
    uploader.onProgressItem = function (fileItem, progress) { };
    uploader.onProgressAll = function (progress) { };
    uploader.onSuccessItem = function (fileItem, response, status, headers) {


    };
    uploader.onErrorItem = function (fileItem, response, status, headers) { };
    uploader.onCancelItem = function (fileItem, response, status, headers) { };
    uploader.onCompleteItem = function (fileItem, response, status, headers) {

        $scope.uploadedfileName = fileItem._file.name;
        response = response.slice(1, -1);
        $scope.fileurl = response;
        $scope.uploadedfileName = response;
    };
    uploader.onCompleteAll = function () {
        alert("Image added to Server no click on save button");
    };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
        //update multi mrp code
        $scope.AddupdateImage = function () {

            if ($scope.uploadedfileName != "") {
                $scope.itemMasterData.LogoUrl = $scope.uploadedfileName;
            } else { alert("Now image, plz upload image "); return false; }
            var url = serviceBase + "api/itemMaster/updateItemImage";
            $http.put(url, $scope.itemMasterData).then(function (results) {
                if (results) {
                    alert("Image updated successfully");
                    $modalInstance.close()
                } else { alert("some thing went wrong"); }
            })
        };

}]);
