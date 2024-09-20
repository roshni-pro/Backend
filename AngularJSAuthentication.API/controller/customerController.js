(function () {
    'use strict';
    angular
        .module('app')
        .controller('customerController', customerController);

    customerController.$inject = ['$scope', 'customerService', 'CityService', 'WarehouseService', "$filter", "$http", "ngTableParams", 'FileUploader', '$modal', '$log', 'ClusterService', "localStorageService"];

    function customerController($scope, customerService, CityService, WarehouseService, $filter, $http, ngTableParams, FileUploader, $modal, $log, ClusterService, localStorageService) {

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


        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        //.................File Uploader method start..................
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
               //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });
        // customer master History get data 

        $scope.CustomerHistroy = function (data) {

            $scope.datacustomermasterHistrory = [];
            var url = serviceBase + "api/Customers/customerhistory?CustomerId=" + data.CustomerId;
            $http.get(url).success(function (response) {

                $scope.datacustomermasterHistrory = response;
                console.log($scope.datacustomermasterHistrory);
                $scope.AddTrack("View(History)", "CustomerId:", data.CustomerId);
            })
                .error(function (data) {
                })
        }






        
        //by Tejas
        $scope.CustomerImages = function (data) {

            var modalInstance;
            console.log('data: ', data);
            localStorage.setItem("ImageCustomerID", data.CustomerId);
            modalInstance = $modal.open(
                {
                    templateUrl: "ImageView.html",
                    controller: "UploadCXimagesController", resolve: { object: function () { return data } }
                })
        };

        //end customer History
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
       {
            $scope.citys = [];
            CityService.getcitys().then(function (results) {
                $scope.citys = results.data;
            }, function (error) { });

            $scope.warehouse = [];
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) { });

            $scope.uploadshow = true;
            $scope.toggle = function () {
                $scope.uploadshow = !$scope.uploadshow;
            }

            var rowCount = 0;



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
            var url = serviceBase + "api/Customers/export";
            $http.get(url).then(function (results) {

                $scope.storesitem = results.data;
                //alasql('SELECT RetailerId,RetailersCode,ShopName,RetailerName,Mobile,Address,Area,Warehouse,ExecutiveName,Emailid,ClusterId,Day,latitute,longitute,BeatNumber,ExecutiveId,ClusterName,[Active],[Deleted] INTO XLSX("Customer.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);

                alasql('SELECT RetailerId,RetailersCode,ShopName,RetailerName,Mobile,Address,Area,city,Warehouse,Emailid,Day,latitute,longitute,ExecutiveId,ExecutiveName,ClusterId,ClusterName, [Active],[Deleted],CustomerVerify,Description, AgentCode,CreationDate INTO XLSX("Customer.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);
            }, function (error) {
            });
        };
        $scope.exportData = function () {

            alasql('SELECT * INTO XLSX("Customer.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };

        $scope.exportDataMissing = function () {

            var url = serviceBase + "api/Customers/getcustomers";
            $http.get(url).then(function (results) {

                $scope.storesitem = results.data;
                //alasql('SELECT RetailerId,RetailersCode,ShopName,RetailerName,Mobile,Address,Area,Warehouse,ExecutiveName,Emailid,ClusterId,Day,latitute,longitute,BeatNumber,ExecutiveId,ClusterName,[Active],[Deleted] INTO XLSX("Customer.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);

                alasql('SELECT Name,ShopName,City,WarehouseName,Skcode,Mobile,AreaName,lat,lg,ResidenceAddressProof,UploadRegistration,AgentCode INTO XLSX("Customer.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);
            }, function (error) {
            });
        };
        //............................Exel export Method.....................//

        $scope.currentPageStores = {};
        $scope.open = function () {
            console.log("Modal opened customer");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModalContent.html",
                    controller: "ModalInstanceCtrl1", resolve: { customer: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");
                    $log.info("Modal dismissed at: " + new Date());
                })
        };

        $scope.customercategorys = {};
        $scope.opendelete = function (data, $index) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerModaldelete.html",
                    controller: "ModalInstanceCtrldeleteCustomer", resolve: { customer: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.data.splice($index, 1);
                $scope.tableParams.reload();
            },
                function () {
                })
        };

        $scope.edit = function (item) {

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
            },
                function () {
                })
        };

        $scope.hub = function (item) {
            
            console.log("Modal opened  ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "Hubtransfer.html",
                    controller: "HubtransferController", resolve: { customer: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        // $scope.customers = [];
        //$http.get(serviceBase + 'api/Customers/InActive').success(function (results) {
        //    $scope.customers = results;
        //    $scope.data = $scope.customers;
        //    $scope.tableParams = new ngTableParams({
        //        page: 1,
        //        count: 50
        //    }, {
        //        total: $scope.data.length,
        //        getData: function ($defer, params) {
        //            var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
        //            orderedData = params.filter() ?
        //                    $filter('filter')(orderedData, params.filter()) :
        //                    orderedData;
        //            $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
        //        }
        //    });
        //}, function (error) {
        //});

        $scope.allcusts = false;
        $scope.customers = [];
        $scope.getallcustomers = function () {

            customerService.getcustomers().then(function (results) {
                $scope.customers = results.data;
                $scope.data = $scope.customers;
                $scope.allcusts = true;
                $scope.tableParams = new ngTableParams({
                    page: 1,
                    count: 50
                }, {
                        total: $scope.data.length,
                        getData: function ($defer, params) {
                            var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                            orderedData = params.filter() ?
                                $filter('filter')(orderedData, params.filter()) :
                                orderedData;
                            $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                        }
                    });
            }, function (error) {
            });
        }
        //missingCustomer

        $scope.getcustomersMissingDetail = function () {
            customerService.getcustomersMissingDetail().then(function (results) {
                $scope.customers = results.data;
                $scope.data = $scope.customers;
                $scope.allcusts = true;
                $scope.tableParams = new ngTableParams({
                    page: 1,
                    count: 50
                }, {
                        total: $scope.data.length,
                        getData: function ($defer, params) {
                            var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                            orderedData = params.filter() ?
                                $filter('filter')(orderedData, params.filter()) :
                                orderedData;
                            $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                        }
                    });
            }, function (error) {
            });
        }
        //end
        //search fn
        $scope.dataforsearch = { Cityid: "", mobile: "", datefrom: "", dateto: "", skcode: "" };
        $scope.Search = function (data) {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            if (data == undefined) {
                alert("Please select one parameter");
                return;
            }
            $scope.dataforsearch.Cityid = data.Cityid;
            $scope.dataforsearch.mobile = data.mobile;
            $scope.dataforsearch.Cityid = data.Cityid;
            $scope.dataforsearch.skcode = data.skcode;
            if (!$('#dat').val()) {
                $scope.dataforsearch.datefrom = '';
                $scope.dataforsearch.dateto = '';
            }
            else {
                $scope.dataforsearch.datefrom = f.val();
                $scope.dataforsearch.dateto = g.val();
            }


            customerService.getfiltereddetails($scope.dataforsearch).then(function (results) {
                $scope.customers = results.data;
                if ($scope.customers.length > 0) {

                    $scope.data = $scope.customers;

                    $scope.allcusts = true;
                    $scope.tableParams = new ngTableParams({
                        page: 1,
                        count: 100,
                        ngTableParams: $scope.customers
                    }, {
                            total: $scope.data.length,
                            getData: function ($defer, params) {
                                var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                                orderedData = params.filter() ?
                                    $filter('filter')(orderedData, params.filter()) :
                                    orderedData;
                                $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                            }
                        });
                }
                else { alert("No customers found for search"); }
            });
        }
        $scope.exportdatewiseData1 = function () {
            $scope.exdatedata = $scope.customers;
            alasql('SELECT CustomerId,CompanyId,CustomerCategoryId,Skcode,ShopName,Warehouseid,Mobile,WarehouseName,Name,Description,AgentCode,CustomerType,CustomerCategoryName,BillingAddress,TypeOfBuissness,ShippingAddress,LandMark,Country,State,Cityid,City,ZipCode,BAGPSCoordinates,SAGPSCoordinates,RefNo,FSAAI,Type,OfficePhone,Emailid,Familymember,CreatedBy,LastModifiedBy,CreatedDate,UpdatedDate,imei,MonthlyTurnOver,ExecutiveId,SizeOfShop,Rating,ClusterId,ClusterName,CustomerVerify INTO XLSX("Customer.xlsx",{headers:true}) FROM ?', [$scope.exdatedata]);

        };

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrl1', ModalInstanceCtrl1);

    ModalInstanceCtrl1.$inject = ["$scope", "CityService", "$filter", "peoplesService", '$http', "$modalInstance", "customer", 'ngAuthSettings', 'WarehouseService', 'CustomerCategoryService', 'ClusterService', 'AreaService', "localStorageService"];

    function ModalInstanceCtrl1($scope, CityService, $filter, peoplesService, $http, $modalInstance, customer, ngAuthSettings, WarehouseService, CustomerCategoryService, ClusterService, AreaService, localStorageService) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.fWhatsappNumber = { Name: '', Description: '' };
        //$scope.CustWarehouse = [];

        

        $scope.CityId = "";
        $scope.ClusterId = "";
        $scope.vm = {};
        //$scope.CustWarehouse = function (CustomerId) {
        $scope.Customer = [];


        $scope.CheckGST = function () {
            var x = document.getElementById("CustomerGST");
            var lc = x.value.length;
            if (lc != 15) {
                alert("GST Number Not Valid Please Re-Enter");
                x.value = "";
            }
        };

        $scope.Customer = function (CustomerId) {
            var url = serviceBase + 'api/Customers/CustWarehouseCustomer?CustomerId=' + CustomerId;
            $http.get(url)
                .success(function (data) {
                    $scope.Customer = data;
                    $scope.CustomerData.AgentCode = $scope.Customer.AgentCode;
                    $scope.CustomerData.ExecutiveId = $scope.Customer.ExecutiveId;
                    $scope.getAgent = data.Agents;
                    $scope.CustomerData.CityId = $scope.CustomerData.Cityid;
                    $scope.CustomerData.ClusterId = $scope.CustomerData.ClusterId;
                });
        };
        //call agent warehouse based
        $scope.AgentsClusterbased = function (clusterId) {
            $scope.getAgent = [];
            return $http.get(serviceBase + 'api/cluster/AgentsClusterBased?clusterId=' + clusterId).then(function (results) {
                $scope.getAgent = results.data;
            });
        };

        $scope.WarehousebasedCluster = function (warehouseid) {
            $scope.Clust = [];
            return $http.get(serviceBase + 'api/cluster/GetClusterWiseWareHouse?warehouseid=' + warehouseid).then(function (results) {
                $scope.Clust = results.data;
            });
        };


        //$scope.getAgent = [];
        //$scope.Salesagent = function (WarehouseId) {
        //    var url = serviceBase + 'api/Agents/Activeagent?WarehouseId=' + WarehouseId; //change because active agent show
        //    $http.get(url)
        //        .success(function (data) {
        //            $scope.getAgent = data;
        //        });
        //};
        //call sales executive warehouse based
        $scope.getExecutive = [];
      
        $scope.Salesasign = function (WarehouseId) {
            
            var url = serviceBase + 'api/AsignDay/Activesalesexe?WarehouseId=' + WarehouseId;//change because active sales executive show 
            //  $scope.Salesagent(WarehouseId);//call sales agent 
            $http.get(url)
                .success(function (data) {
                    $scope.getExecutive = data;

                });
        };
        //call all city 

        $scope.citydata = [];
        CityService.getcitys().then(function (results) {
            $scope.citydata = results.data;
        }, function (error) {

        });
        // call city based warehouse 
        $scope.warehouse = [];
        $scope.warehousecitybased = function (CityId) {
            WarehouseService.warehousecitybased(CityId).then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            });
        };
        //this function call only add customer  
        $scope.SelectFilterWh = function (data) {

            $scope.AreaCityBased(data); // call city based area if add customer
            // $scope.Clustercitybased(data);// call city based cluster
            $scope.warehousecitybased(data);//call city based warehouses
            //$scope.AgentsClusterbased($scope.data.clusterId);
            //$scope.filterData = $filter('filter')($scope.warehouse, { Cityid: data });
            //$scope.WHData = [];
            //$scope.WhData = $scope.filterData;

        };
        $scope.SelectFilterClust = function (data) {
            //$scope.CluseterWiseWarehosue(data);
            $scope.AgentsClusterbased(data);

        };
        $scope.SelectFilterWarHouse = function (data) {
            $scope.customerorder();//by Sudhir    
            $scope.WarehousebasedCluster(data);
            $scope.Salesasign(data);

        };

        //$scope.SelectFilterCluster = function (data) {

        //    $ // call city based area if add customer
        //    $scope.Clustercitybased(data);// call city based cluster
        //    $scope.warehousecitybased(data);//call city based warehouses
        //    //$scope.AgentsClusterbased($scope.data.clusterId);
        //    //$scope.filterData = $filter('filter')($scope.warehouse, { Cityid: data });
        //    //$scope.WHData = [];
        //    //$scope.WhData = $scope.filterData;

        //};

        //CustomerCategoryService.getcustomercategorys().then(function (results) {
        //    $scope.customercategorys = results.data;       
        //}, function (error) {
        //    //alert(error.data.message);
        //});

        $scope.getcluster = [];

        //Pravesh
        function ReloadPage() {
            location.reload();
        }

        // call city based  clusters
        $scope.Clusters = [];
        $scope.Clustercitybased = function (CityId) {
            ClusterService.getCitycluster(CityId).then(function (results) {
                $scope.getcluster = results.data;
                $scope.getAgent = [];

                if (results.data) {
                    for (var i = 0; i < results.data.length; i++) {
                        $scope.getAgent.push(results.data[i].Agents);
                    }
                }


            }, function (error) {
            });
        };



        // call city  based area
        $scope.Area = [];
        $scope.AreaCityBased = function (CityId) {
            AreaService.getareacityid(CityId).then(function (results) {
                $scope.Area = results.data;
            }, function (error) {
            });
        }
        //this is call only edit customer

        if (customer) {

            $scope.CustomerData = customer;
            //$scope.CustWarehouse($scope.CustomerData.CustomerId);
            $scope.Customer($scope.CustomerData.CustomerId);
            // $scope.Salesagent($scope.CustomerData.Warehouseid);
            $scope.AreaCityBased($scope.CustomerData.Cityid);
            $scope.warehousecitybased($scope.CustomerData.Cityid);
            //$scope.Clustercitybased($scope.CustomerData.Cityid);
            //$scope.CluseterWiseWarehosue($scope.CustomerData.ClusterId);
            $scope.Salesasign($scope.CustomerData.Warehouseid);
            $scope.WarehousebasedCluster($scope.CustomerData.Warehouseid);
            $scope.AgentsClusterbased($scope.CustomerData.clusterId);

        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.examplemodel = [];
        $scope.exampledata = $scope.Area;
        $scope.examplesettings = {
            displayProp: 'AreaName', idProp: 'AreaName',
            scrollableHeight: '250px',
            scrollableWidth: '500px',
            enableSearch: true,
            scrollable: true
        };

        $scope.CheckCustomerMobile = function (Mobile) {
            var url = serviceBase + "api/Customers/Mobile?Mobile=" + Mobile;
            $http.get(url)
                .success(function (data) {
                    if (data == null) {
                    }
                    else {
                        alert("Mobile Number Is already Exist");
                        //$scope.CustomerData.Mobile = '';
                    }
                    console.log(data);
                });
        }
        $scope.CheckCustomerEmail = function (Email) {
            var url = serviceBase + "api/Customers/Email?Email=" + Email;
            $http.get(url)
                .success(function (data) {
                    if (data == null) {
                    }
                    else {
                        alert("Email Id Is already Exist");
                        //$scope.CustomerData.Emailid = '';
                    }
                    console.log(data);
                });
        }
        //Pravesh
        $scope.GetClusterByLatLong = [];
        $scope.GetClusterByLatLong = function (lat, long) {

            var url = serviceBase + 'api/Customers/GetClusters?lat=' + lat + '&longt=' + long;
            $http.get(url)
                .success(function (data) {

                    //$scope.CustWarehouse = data;
                    $scope.Customer = data;
                    //$scope.CustomerData.AgentCode = $scope.CustWarehouse.AgentCode;
                    //$scope.CustomerData.ExecutiveId = $scope.CustWarehouse.ExecutiveId;

                });
        }
        // Pravesh
        $scope.Customerstatus = [
            { type: 'Full Verified' },
            { type: 'Not Verified' },
        ];

        $scope.SubType = [
      
            { atype: 'Printed' },
            { atype: 'Print Pending' },
            
        ];
        $scope.Subtype = [
            { stype: 'Testing' },
            { stype: 'No Document' },
            { stype: 'Inform to Upload Document' },
            { stype: 'Wrong Document' },
            { stype: 'Invalid Mobile Number' },
            { stype: 'Mis-Match of Document' },
            { stype: 'Incorrect GST Number' },
            { stype: 'Incorrect License Number' },
            { stype: 'Image Not Clear' },
            { stype: 'Double SK Code' },
            { stype: 'Not a Retailer' },
            { stype: 'Shop Closed' },
            { stype: 'Not Interested' },
            { stype: 'Inactive' },
           
        ];


        $scope.AddCustomer = function (data) {
            $scope.dataselect = [];
            $scope.CustomerData = data;
            var ade = $scope.CustomerData;


            var area = [];
            _.each($scope.examplemodel, function (o2) {
                var Row = o2.id;
                area.push(Row);
            });
            var reggst = /^([0-9]){2}([a-zA-Z]){5}([0-9]){4}([a-zA-Z]){1}([0-9]){1}([a-zA-Z]){2}?$/;
            var abs = $('#Customer-mobile').val().length;
            var arr = $scope.CustomerData.RefNo;
            if (data.ShopName == null) {
                alert('Please Enter ShopName ');
            }
            else if (arr != null && !reggst.test(arr)) {
                alert('GST Identification Number is not valid. It should be in this "11AAAAA1111Z1AA" format');
                this.focus();
            }
            else if (data.Cityid == null) {
                alert('Please Select City ');
            }
            else if (data.CustomerVerify == null) {
                alert('Please Select CustomerVerify ');
            }
            else if (data.BillingAddress == null) {
                alert('Please Enter BillingAddress ');
            }
            else if (data.Mobile == null) {
                alert('Please Enter Mobile Number ');
            }
            else if (abs < 10 || abs > 10) {
                alert('Please Enter Mobile Number in 10 digit ');
            }
            else if (data.DOB == null) {
                alert('Please Select Date of brith ');
            }
            else {

                if ($scope.CustomerData.Active == true) {
                    $scope.CustomerData.IsHide = false;
                }

                $("#AddCustomer").prop("disabled", true);
                var url = serviceBase + "api/customers";

                var dataToPost = {
                    CustomerCategoryId: $scope.CustomerData.CustomerCategoryId,
                    AgentCode: $scope.CustomerData.AgentCode,
                    Name: $scope.CustomerData.Name,
                    ShopName: $scope.CustomerData.ShopName,
                    Skcode: $scope.CustomerData.Skcode,
                    LandMark: area[0],
                    ExecutiveId: $scope.CustomerData.ExecutiveId,
                    Password: $scope.CustomerData.Password,
                    Description: $scope.CustomerData.Description,
                    CustomerType: $scope.CustomerData.CustomerType,
                    CustomerCategoryName: $scope.CustomerData.CustomerCategoryName,
                    BillingAddress: $scope.CustomerData.BillingAddress,
                    ShippingAddress: $scope.CustomerData.ShippingAddress,
                    Cityid: $scope.CustomerData.Cityid,
                    Mobile: $scope.CustomerData.Mobile,
                    Warehouseid: $scope.CustomerData.Warehouseid,
                    BAGPSCoordinates: $scope.CustomerData.BAGPSCoordinates,
                    SAGPSCoordinates: $scope.CustomerData.SAGPSCoordinates,
                    RefNo: $scope.CustomerData.RefNo,
                    FSAAI: $scope.CustomerData.FSAAI,
                    OfficePhone: $scope.CustomerData.OfficePhone,
                    Emailid: $scope.CustomerData.Emailid,
                    Familymember: $scope.CustomerData.Familymember,
                    CreatedDate: $scope.CustomerData.CreatedDate,
                    UpdatedDate: $scope.CustomerData.UpdatedDate,
                    CreatedBy: $scope.CustomerData.CreatedBy,
                    DOB: $scope.CustomerData.DOB,
                    LastModifiedBy: $scope.CustomerData.LastModifiedBy,
                    Active: $scope.CustomerData.Active,
                    IsHide: $scope.CustomerData.IsHide, //Praveen
                    SizeOfShop: $scope.CustomerData.SizeOfShop,
                    lat: $scope.CustomerData.lat,
                    lg: $scope.CustomerData.lg,
                    MonthlyTurnOver: $scope.CustomerData.MonthlyTurnOver,
                    AnniversaryDate: data.AnniversaryDate,                /* < !--tejas 29 - 05 - 2019-- >*/
                    WhatsappNumber: data.WhatsappNumber,                       /*< !--tejas 29 - 05 - 2019-- >*/
                    LicenseNumber: data.LicenseNumber,                      /*< !--tejas 29 - 05 - 2019-- >*/
                    CustomerVerify: $scope.CustomerData.CustomerVerify,
                    StatusSubType: data.StatusSubType,
                    ClusterId: $scope.CustomerData.ClusterId


                };
                $http.post(url, dataToPost)
                    .success(function (data) {
                        alert("Customer Successfully Created");
                        window.location.reload();

                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            $modalInstance.close(data);
                        }
                    })
                    .error(function (data) {
                    })
            }

        };



        $scope.PutCustomer = function (data) {
            
            var area = [];
            var reggst = /^([0-9]){2}([a-zA-Z]){5}([0-9]){4}([a-zA-Z]){1}([0-9]){1}([a-zA-Z]){2}?$/;
            _.each($scope.examplemodel, function (o2) {
                var Row = o2.id;
                area.push(Row);
            });
            //$scope.CustomerData = {};
            //if (customer) {

            //    $scope.CustomerData = customer;
            //}
            var abs = $('#Customer-mobile').val().length;
            var arr = $scope.CustomerData.RefNo;
            if (data.ShopName == null) {
                alert('Please Enter ShopName ');
            }
            //else if (arr != null && !reggst.test(arr)) {
            //    alert('GST Identification Number is not valid. It should be in this "11AAAAA1111Z1AA" format');
            //    this.focus();
            //}
            //else if (area.length == 0 && data.LandMark == null && data.LandMark == "null" && data.LandMark == "") {
            //    alert('Please Enter LandMark/Area ');
            //}
            else if (data.Cityid == null) {
                alert('Please Select City ');
            }
            //else if (data.CustomerVerify == null) {
            //    alert('Please Select CustomerVerify ');
            //}
            else if (data.BillingAddress == null) {
                alert('Please Enter BillingAddress ');
            }
            //else if (data.Active == false && data.Description == null) {

            //    alert('Please Enter Decription');
            //}
            else if (data.Mobile == null) {

                alert('Please Enter Mobile Number ');
            }
            else if (data.StatusSubType == null || data.StatusSubType=='') {
                alert ('Please Select SubType');
            }
            else if (abs < 10 || abs > 10) {

                alert('Please Enter Mobile Number in 10 digit ');
            }
            else {
                if (area.length == 0) {
                    area[0] = data.LandMark;
                }
                //$("#PutCustomer").prop("disabled", true);
                //if (data.Active == true) {
                //    $scope.CustomerData.Description = "";
                //}

                var cluster = [];
                if ($("#Clusterid").val()) {
                    cluster = $("#Clusterid").val().split(',');
                } else {
                    cluster[0] = "";
                    cluster[1] = "";
                }

                if ($scope.CustomerData.Active == true) {
                    $scope.CustomerData.IsHide = false;
                }

                var url = serviceBase + "api/customers";

                var dataToPost = {
                    CustomerId: $scope.CustomerData.CustomerId,
                    AgentCode: $scope.CustomerData.AgentCode,
                    CustomerCategoryId: $scope.CustomerData.CustomerCategoryId,
                    Name: $scope.CustomerData.Name,
                    Password: $scope.CustomerData.Password,
                    LandMark: area[0],
                    Description: $scope.CustomerData.Description,
                    CustomerType: $scope.CustomerData.CustomerType,
                    ShopName: $scope.CustomerData.ShopName,
                    CustomerCategoryName: $scope.CustomerData.CustomerCategoryName,
                    BillingAddress: $scope.CustomerData.BillingAddress,
                    ShippingAddress: $scope.CustomerData.ShippingAddress,
                    City: $scope.CustomerData.City,
                    Cityid: $scope.CustomerData.Cityid,
                    BAGPSCoordinates: $scope.CustomerData.BAGPSCoordinates,
                    SAGPSCoordinates: $scope.CustomerData.SAGPSCoordinates,
                    RefNo: $scope.CustomerData.RefNo,
                    Mobile: $scope.CustomerData.Mobile,
                    Decription: $scope.CustomerData.Decription,
                    Warehouseid: $scope.CustomerData.Warehouseid,
                    OfficePhone: $scope.CustomerData.OfficePhone,
                    Emailid: $scope.CustomerData.Emailid,
                    Familymember: $scope.CustomerData.Familymember,
                    Active: $scope.CustomerData.Active,
                    IsHide: $scope.CustomerData.IsHide, //Praveen
                    ExecutiveId: $scope.CustomerData.ExecutiveId,
                    Skcode: $scope.CustomerData.Skcode,
                    lat: $scope.CustomerData.lat,
                    lg: $scope.CustomerData.lg,
                    DOB: data.DOB,                                            /*< !--tejas 29 - 05 - 2019-- >   */
                    AnniversaryDate: data.AnniversaryDate,                /* < !--tejas 29 - 05 - 2019-- >*/
                    WhatsappNumber: data.WhatsappNumber,                       /*< !--tejas 29 - 05 - 2019-- >*/
                    LicenseNumber: data.LicenseNumber,                       /*< !--tejas 29 - 05 - 2019-- >*/
                    CustomerVerify: $scope.CustomerData.CustomerVerify,
                    StatusSubType: data.StatusSubType,
                    LastModifiedBy: $scope.CustomerData.LastModifiedBy,
                    ClusterId: $scope.CustomerData.ClusterId

                    //ClusterName: cluster[1] 
                    //ClusterId: $scope.CustomerData.ClusterId
                };
                $http.put(url, dataToPost)
                    .success(function (data) {



                        alert("Customer Updated");

                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            $modalInstance.close(data);
                        }
                        // window.location.reload();
                    })
                    .error(function (data) {
                    })
            }
        };
    }
})();



(function () {
    'use strict';
    
    angular
        .module('app')
        .controller('HubtransferController', HubtransferController);

    HubtransferController.$inject = ["$scope", '$http', 'WarehouseService', "$modalInstance", 'ngAuthSettings', "customer"];

    function HubtransferController($scope, $http, WarehouseService, $modalInstance, ngAuthSettings, customer) {
        
        $scope.CustomerNew = customer.CustomerId;
        var cityid = customer.Cityid;
        var Warehouseid = customer.Warehouseid;

        $scope.warehouse = [];
        WarehouseService.warehousecitybased(cityid).then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) { });
        //by sudhir
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.customerorder = function (cust) {
            
            $scope.orderdata = [];
            var url = serviceBase + 'api/Customers/Customerdata?Customerid=' + $scope.CustomerNew + "&warehouseid=" + cust;
            $http.get(url)
                .success(function (data) {
                    
                    $scope.orderdata = data;

                });
        };
        //
        $scope.save = function (data) {
            
            $scope.Hubtransfer = data;
            var url = serviceBase + "api/customers/Hubtransferdata?Customerid=" + $scope.CustomerNew + "&warehouseid=" + data.Warehouseid;
            var dataToPost = {       
            }
            $http.put(url, dataToPost)
                .success(function (data) {
                    
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        alert(data)
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {
                })
        }

    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteCustomer', ModalInstanceCtrldeleteCustomer);

    ModalInstanceCtrldeleteCustomer.$inject = ["$scope", '$http', "$modalInstance", "customerService", 'ngAuthSettings', "customer"];

    function ModalInstanceCtrldeleteCustomer($scope, $http, $modalInstance, customerService, ngAuthSettings, customer) {
        function ReloadPage() {
            location.reload();
        }
        $scope.CustomerData = {};
        if (customer) {
            $scope.CustomerData = customer;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.deletecustomers = function (dataToPost, index) {
            customerService.deletecustomers(dataToPost).then(function (results) {
                $modalInstance.close(dataToPost);
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
        .controller('UploadCXimagesController', UploadCXimagesController);

    UploadCXimagesController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "NewsService", "FileUploader", "unitMasterService", "localStorageService", "$modal"];

    function UploadCXimagesController($scope, $http, ngAuthSettings, $modalInstance, object, ItemService, FileUploader, unitMasterService, localStorageService, $modal) {

        ///tejas
        if (object) {
            $scope.CustomerDataOnject = object;
        }
        $scope.baseurl = serviceBase + "/UploadedImages/";


        console.log('$scope.CustomerID:', $scope.CustomerID);

        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.datacustomermasterImage = [];
        var url = serviceBase + "api/Customers/customerImages?CustomerId=" + $scope.CustomerDataOnject.CustomerId;
        $http.get(url).success(function (response) {
            $scope.datacustomermasterImage = response;
            console.log($scope.datacustomermasterImages);

        })


        //#region   GST Img
        ///tejas
        var uploader1 = $scope.uploader1 = new FileUploader({

            url: serviceBase + 'api/logoUpload/UploadCXimagesGST?CustomerId=' + $scope.CustomerDataOnject.CustomerId
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



        };
        uploader1.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
            alert("Image Upload failed");

        };
        uploader1.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
            alert("Image Uploaded Failed");


        };
        uploader1.onCompleteItem = function (fileItem, response, status, headers) {
            console.log("File Name :" + fileItem._file.name);
            alert("Image Uploaded Successfully");

        };
        uploader1.onCompleteAll = function () {
            console.info('onCompleteAll');
        };
        //#endregion

        //#region Shop Img
        ///tejas
        var uploader2 = $scope.uploader2 = new FileUploader({

            url: serviceBase + 'api/logoUpload/UploadCXimagesSHOP?CustomerId=' + $scope.CustomerDataOnject.CustomerId
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

        };

        uploader2.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader2.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
            alert("Image Uploaded Failed");
        };
        uploader2.onCompleteItem = function (fileItem, response, status, headers) {
            $scope.uploadedfileName = fileItem._file.name;
            alert("Image Uploaded Successfully");
        };
        uploader2.onCompleteAll = function () {
            console.info('onCompleteAll');
        };

        //#endregion

        //#region ReG image 
        ///tejas
        var uploader3 = $scope.uploader3 = new FileUploader({

            url: serviceBase + 'api/logoUpload/UploadCXimagesREG?CustomerId=' + $scope.CustomerDataOnject.CustomerId

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
        };
        uploader3.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader3.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
            alert("Image Uploaded Failed");

        };
        uploader3.onCompleteItem = function (fileItem, response, status, headers) {
            $scope.uploadedfileName = fileItem._file.name;
            alert("Image Uploaded Successfully");

        };
        uploader3.onCompleteAll = function () {
            console.info('onCompleteAll');
        };
        //#endregion
    }
})();
