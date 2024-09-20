

(function () {
    //'use strict';

    angular
        .module('app')
        .controller('peoplesController', peoplesController);

    peoplesController.$inject = ['$scope', 'peoplesService', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService', 'ClusterService'];

    function peoplesController($scope, peoplesService, CityService, StateService, $filter, $http, ngTableParams, $modal, WarehouseService, ClusterService) {

        function sendFileToServer(formData, status) {
            var uploadURL = "/api/peopleupload/post"; //Upload URL
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
            if (rowCount % 2 === 0) row = "even";
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

        console.log("People Controller reached");
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            //.................File Uploader method start..................    =
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

            ///PeopleImages
            $scope.PeopleImages = function (data) {

                var modalInstance;
                console.log('data: ', data);
                localStorage.setItem("ImageCustomerID", data.PeopleID);
                modalInstance = $modal.open(
                    {
                        templateUrl: "ImageView.html",
                        controller: "PeopleimagesController", resolve: { object: function () { return data } }
                    })

            };

            $scope.citys = [];
            CityService.getcitys().then(function (results) {

                $scope.citys = results.data;
            }, function (error) {
            });

            $scope.warehouse = [];
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;

            }, function (error) {
            });

            //// People History get data 
            //$scope.PeopleHistroy = function (data) {

            //    $scope.dataPeopleHistrory = [];
            //    var url = serviceBase + "api/Peoples/History?PeopleId=" + data.PeopleID;
            //    $http.get(url).success(function (response) {

            //        $scope.dataPeopleHistrory = response;
            //        console.log($scope.dataPeopleHistrory);

            //    })
            //        .error(function (data) {
            //        })
            //}

            //$scope.HistroyDetails = function (data) {

            //    $scope.dataHistrory = [];
            //    var url = serviceBase + "api/Peoples/HistoryById?Id=" + data.Id;
            //    $http.get(url).success(function (response) {

            //        $scope.dataHistrory = response;
            //        console.log($scope.dataPeopleByidHistrory);

            //    })
            //        .error(function (data) {
            //        })
            //}
            ////end People History
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
            $scope.currentPageStores = {};
            $scope.open = function () {

                console.log("Modal opened people");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myPeopleModal.html",
                        controller: "ModalInstanceCtrlPeople", resolve: { people: function () { return $scope.items } }
                    });
                modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };
            $scope.edit = function (item) {
                console.log("Edit Dialog called people");
                var modalInstance;
                
                modalInstance = $modal.open(
              
                    {
                        templateUrl: "myPeopleModalPut.html",
                        controller: "ModalInstanceCtrlPeople", resolve: { people: function () { return item } }
                    });
                modalInstance.result.then(function (selectedItem) {

                    $scope.peoples.push(selectedItem);
                    _.find($scope.peoples, function (people) {
                        if (people.id === selectedItem.id) {
                            people = selectedItem;
                        }
                    });
                    $scope.peoples = _.sortBy($scope.peoples, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };
            $scope.opendelete = function (data, $index) {

                console.log(data);
                console.log("Delete Dialog called for people");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModaldeletepeople.html",
                        controller: "ModalInstanceCtrldeletepeople", resolve: { people: function () { return data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            $scope.peoples = [];
            peoplesService.getpeoples().then(function (results) {

                $scope.peoples = results.data;
                console.log("Got people collection");
                console.log($scope.peoples);
                $scope.callmethod();

                $scope.AddData = function () {

                    var url = serviceBase + "api/trackuser?action=View&item=People";
                    $http.post(url).success(function (results) {

                    });

                }
                $scope.AddData();
            }, function (error) {
            });

            $scope.warehouse = [];
            WarehouseService.getwarehouse().then(function (results) {

                $scope.warehouse = results.data;
            }, function (error) {
                });

            $scope.Change = function (item) {
     
                console.log("Edit Dialog called city");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "Changepassword.html",
                        controller: "ModalInstanceChange", resolve: { people: function () { return item } }
                    }), modalInstance.result.then(function (selectedItem) {

                        $scope.city.push(selectedItem);
                    _.find($scope.people, function (people) {
                        if (people.id == selectedItem.id) {
                            people = selectedItem;
                            }
                        });

                    $scope.people = _.sortBy($scope.people, 'Id').reverse();
                        $scope.selected = selectedItem;

                    },
                        function () {
                            console.log("Cancel Condintion");

                        })
            };

            $scope.callmethod = function () {

                var init;
                $scope.stores = $scope.peoples;

                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";

              
                $scope.numPerPageOpt = [20, 30, 50, 200];
                $scope.numPerPage = $scope.numPerPageOpt[1];
                $scope.currentPage = 1;
                $scope.currentPageStores = [];
                $scope.search(); $scope.select(1);
            }

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                start = (page - 1) * $scope.numPerPage;
                end = start + $scope.numPerPage;
                $scope.currentPageStores = $scope.filteredStores.slice(start, end);
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

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlPeople', ModalInstanceCtrlPeople);

    ModalInstanceCtrlPeople.$inject = ["$scope", '$http', "designationservice", "DepartmentService", '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', 'FileUploader', "$modalInstance", "people", 'WarehouseService', 'authService', 'ClusterService'];

    function ModalInstanceCtrlPeople($scope, $http, designationservice, DepartmentService, $modal, ngAuthSettings, peoplesService, CityService, StateService, FileUploader, $modalInstance, people, WarehouseService, authService, ClusterService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("People");
        

        $scope.showform = true;
        $scope.showform1 = false;
        $scope.showform2 = false;


        $scope.warehouse = {};
        // function for get Warehosues from warehouse service
        $scope.getWarehosues = function () {
            WarehouseService.getwarehousewokpp().then(function (results) {
                if (results.data.length > 0) {
                    for (var a = 0; a < results.data.length; a++) {
                        results.data[a].WarehouseName = results.data[a].WarehouseName + " " + results.data[a].CityName;
                    }
                    $scope.warehouse = results.data;
                }
            }, function (error) {
            });
        };
        $scope.getWarehosues();

        $scope.DetailsForm = function () {
            $scope.showform = true;
            $scope.showform1 = false;
            $scope.showform2 = false;
        }


        $scope.onStateChange = function () {
            console.log('PeopleData.Stateid: ' + $scope.PeopleData.Stateid);
            CityService.getByStateID($scope.PeopleData.Stateid).then(function (result) {
                $scope.citys = result.data;
            })
        }

        $scope.onWHChange = function () {
            console.log('PeopleData.Cityid: ' + $scope.PeopleData.Cityid);
            WarehouseService.warehousecitybased($scope.PeopleData.Cityid).then(function (result) {
                $scope.warehouse = result.data;
            })
        }

        $scope.Salary_Calculation = function (data) {


            $scope.PeopleData.Hra_Salary = (parseInt(data) * 40) / 100;
            $scope.PeopleData.Lta_Salary = 0;
            $scope.PeopleData.CA_Salary = 0;
            $scope.PeopleData.DA_Salary = 0;
            $scope.PeopleData.PF_Salary = (((parseInt(data) * 75) / 100) * 12) / 100;
            $scope.PeopleData.ESI_Salary = (parseInt(data) * 6.5) / 100;
            $scope.PeopleData.B_Salary = 0;
            $scope.PeopleData.Y_Incentive = 0;
            $scope.PeopleData.M_Incentive = 0;

        }

        //Validation Form Salary
        $scope.SalaryForm = function () {
            $scope.showform = false;
            $scope.showform1 = true;
            $scope.showform2 = false;
        }
        //Validation Form Document
        $scope.DocumentForm = function () {
            $scope.showform = false;
            $scope.showform1 = false;
            $scope.showform2 = true;
        }
        $scope.PeopleData = {}; var alrt = {};
        if (people) {


            var Mobile = parseInt(people.Mobile);
            people.Mobile = Mobile;
            $scope.PeopleData = people;
        }
        $scope.citys = [];
        $scope.getRole = function () {
            var url = serviceBase + "api/usersroles/GetAllRoles";
            $http.get(url)
                .success(function (data) {
                    $scope.roles = data;
                    console.log(data);
                }, function (error) { });
        }
        $scope.getRole();
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) {
        });
        $scope.Designation = [];
        designationservice.getdesignations().then(function (results) {
            $scope.Designation = results.data;
        }, function (error) {
        });

        $scope.Departments = [];
        DepartmentService.getdepartments().then(function (results) {
            $scope.Departments = results.data;
        }, function (error) {
        });
        $scope.states = [];
        StateService.getstates().then(function (results) {
            $scope.states = results.data;
        }, function (error) {
        });
        //$scope.warehouse = [];
        //WarehouseService.getwarehouse().then(function (results) {
        //    $scope.warehouse = results.data;
        //}, function (error) {
        //});

        $scope.Cluster = [];
        ClusterService.getcluster().then(function (results) {
            $scope.Cluster = results.data;

        }, function (error) {
        });
        //........multi.........//
        $scope.clusterModel = [];

        $scope.clusterModel = $scope.Cluster;
        $scope.clusterSetting = {
            displayProp: 'ClusterName', idProp: 'ClusterId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };
        // For Validation 
        $scope.CheckEmail = function (Email) {

            var url = serviceBase + "api/Peoples/Email?Email=" + Email;
            $http.get(url)
                .success(function (data) {
                    if (data === null) {
                    }
                    else {
                        alert("Email Is already Exist");
                        //$scope.PeopleData.Email = '';
                    }
                    console.log(data);
                });
        }

        $scope.CheckMobile = function (Mobile) {

            var url = serviceBase + "api/Peoples/Mobile?Mobile=" + Mobile;
            $http.get(url)
                .success(function (data) {
                    if (data === null) {
                    }
                    else {
                        alert("Mobile Number Is already Exist");

                    }
                    console.log(data);
                });


        }


        $scope.baseurl = serviceBase + "/UploadDocuments/";

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.AddPeople = function (PeopleData) {


            //if (PeopleData.Email != null) {
            //    PeopleData.Email = PeopleData.Email + '@shopkirana.com';
            //}
            //if (PeopleData.PeopleFirstName == null || PeopleData.PeopleFirstName == "") {
            //    alert('Please Enter First Name');
            //    $modalInstance.open();
            //}
            //else if (PeopleData.PeopleLastName == null || PeopleData.PeopleLastName == "") {
            //    alert('Please Enter Last Name');
            //    $modalInstance.open();
            //}
            //else if (PeopleData.Email == null || PeopleData.Email == "") {
            //    alert('Please Enter Email');
            //    $modalInstance.open();
            //}
            //else if (PeopleData.Password == null || PeopleData.Password == "") {
            //    alert('Please Enter Password');
            //    $modalInstance.open();
            //}
            //else if (PeopleData.Mobile == null || PeopleData.Mobile == "") {
            //    alert('Please Enter Mobile');
            //    $modalInstance.open();
            //}
            //else if (PeopleData.Stateid == "") {
            //    alert('Please Enter State');
            //    $modalInstance.open();
            //}
            //else if (PeopleData.Cityid == "") {
            //    alert('Please Enter City');
            //    $modalInstance.open();
            //}
            //else if (PeopleData.WarehouseId == "") {
            //    alert('Please Enter Warehouse');
            //    $modalInstance.open();
            //}
            //else if (PeopleData.Department == "") {
            //    alert('Please Enter Department');
            //    $modalInstance.open();
            //}


            console.log("Modal opened people");
            $modalInstance.close();
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "Second.html",
                    controller: "ModalInstanceCtrlPeopleSecond", resolve: { people1: function () { return PeopleData } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");
                })
            //authService.saveRegistrationpeople($scope.PeopleData).then(function (response) {
            //    console.log(response);
            //    $modalInstance.close(response);
            //    $scope.page = response.config.data.Department;
            //    if (response.status == "200") {
            //        
            //        $scope.AddData = function () {
            //            
            //            var url = serviceBase + "api/trackuser?action=AddPeople&item=" + $scope.page;
            //            $http.post(url).success(function (results) {

            //            });
            //        }
            //        
            //        $scope.AddData();

            //        alrt.msg = "Record has been Save successfully";


            //        $modal.open(
            //                        {
            //                            templateUrl: "PopUpModel.html",
            //                            controller: "PopUpController", resolve: { message: function () { return alrt } }
            //                        }), modalInstance.result.then(function (selectedItem) {
            //                        },
            //                    function () {
            //                        console.log("Cancel Condintion");
            //                    })
            //    }
            //});
        };

        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };


        $('.agentmultiselect').multiselect();
        $scope.dataselect = [];

        
        var url = serviceBase + "api/Peoples/GetWarehouseEditPeople?Peopleid=" + $scope.PeopleData.PeopleID;
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


        $scope.PutPeople = function (data) {
            
            var ids = [];
            _.each($scope.clusterModel, function (o2) {
                console.log(o2);
                for (var i = 0; i < $scope.Cluster.length; i++) {
                    if ($scope.Cluster[i].ClusterId === o2.id) {
                        var Row =
                        {
                            "id": o2.id
                        };
                        ids.push(Row);
                    }
                }
            });

            //var wids = [];
            //_.each($scope.examplemodel, function (o2) {
            //    wids.push(o2.id);
            //});

            if (data.PeopleFirstName === null || data.PeopleFirstName === "") {
                alert('Please Enter First Name');
                $modalInstance.open();
            }
            else if (data.PeopleLastName === null || data.PeopleLastName === "") {
                alert('Please Enter Last Name');
                $modalInstance.open();
            }
            else if (data.Email === null || data.Email === "") {
                alert('Please Enter Email');
                $modalInstance.open();
            }
            else if (data.Mobile === null || data.Mobile === "") {
                alert('Please Enter Mobile');
                $modalInstance.open();
            }
            else if (data.Stateid === "") {
                alert('Please Enter State');
                $modalInstance.open();
            }
            else if (data.Cityid === "") {
                alert('Please Enter City');
                $modalInstance.open();
            }
            else if (data.Department === "") {
                alert('Please Enter Department');
                $modalInstance.open();
            }
            $scope.PeopleData = {};
            var datatopost = {
                Mobile: data.Mobile,
                ids: ids
            };
            $scope.clustdata = function () {

                var url = serviceBase + "api/Peoples/mapcluster";
                $http.post(url, datatopost).success(function (results) {
                    alert("Record Inserted Successfull");
                    window.location.reload();
                });
            };
            $scope.clustdata();
            if (people) {
                $scope.PeopleData = people;
                console.log("found Puttt People");
                console.log(people);
                console.log($scope.PeopleData);
                $scope.ok = function () { $modalInstance.close(); };
                $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
                console.log("Update People");
                $scope.AddData = function () {
                    var url = serviceBase + "api/trackuser?action=Edit&item=PeopleID:" + $scope.PeopleData.PeopleID;
                    $http.post(url).success(function (results) {
                    });
                }
                $scope.AddData();
               
                var url = serviceBase + "api/Peoples";
                // var P=$scope.PeopleData.Permissions,
                var dataToPost = {
                    PeopleID: $scope.PeopleData.PeopleID,
                    Active: $scope.PeopleData.Active,
                    Stateid: $scope.PeopleData.Stateid,
                    state: $scope.PeopleData.state,
                    Cityid: $scope.PeopleData.Cityid,
                    city: $scope.PeopleData.city,
                    Mobile: $scope.PeopleData.Mobile,
                    PeopleFirstName: $scope.PeopleData.PeopleFirstName,
                    PeopleLastName: $scope.PeopleData.PeopleLastName,
                    WarehouseId: $scope.PeopleData.WarehouseId,
                    CreatedDate: $scope.PeopleData.CreatedDate,
                    UpdatedDate: $scope.PeopleData.UpdatedDate,
                    CreatedBy: $scope.PeopleData.CreatedBy,
                    UpdateBy: $scope.PeopleData.UpdateBy,
                    Email: $scope.PeopleData.Email,
                    Department: $scope.PeopleData.Department,
                    Desgination: $scope.PeopleData.Desgination,
                    BillableRate: $scope.PeopleData.BillableRate,
                    CostRate: $scope.PeopleData.CostRate,
                    Permissions: $scope.PeopleData.Permissions,
                    Skcode: $scope.PeopleData.Skcode,
                    Type: $scope.PeopleData.Department,
                    Salesexecutivetype: $scope.PeopleData.Salesexecutivetype,
                    AgentCode: $scope.PeopleData.AgentCode,
                    DOB: $scope.PeopleData.DOB,
                    Status: $scope.PeopleData.Status,
                    DataOfMarriage: $scope.PeopleData.DataOfMarriage,
                    EndDate: $scope.PeopleData.EndDate,
                    DataOfJoin: $scope.PeopleData.DataOfJoin,
                    Unit: $scope.PeopleData.Unit,
                    Reporting: $scope.PeopleData.Reporting,
                    Salary: $scope.PeopleData.Salary,
                    B_Salary: $scope.PeopleData.B_Salary,
                    Hra_Salary: $scope.PeopleData.Hra_Salary,
                    CA_Salary: $scope.PeopleData.CA_Salary,
                    DA_Salary: $scope.PeopleData.DA_Salary,
                    Lta_Salary: $scope.PeopleData.Lta_Salary,
                    PF_Salary: $scope.PeopleData.PF_Salary,
                    ESI_Salary: $scope.PeopleData.ESI_Salary,
                    M_Incentive: $scope.PeopleData.M_Incentive,
                    Id_Proof: $scope.PeopleData.Id_Proof,
                    Address_Proof: $scope.PeopleData.Address_Proof,
                    MarkSheet: $scope.PeopleData.MarkSheet,
                    Pre_SalarySlip: $scope.PeopleData.Pre_SalarySlip,
                    DepositAmount: $scope.PeopleData.DepositAmount,
                    tempdel: $scope.PeopleData.tempdel,
                    Warehouses: $scope.vm.examplemodel
                };

                console.log(dataToPost);
                $http.put(url, dataToPost)
                    .success(function (data) {
                        console.log("Error Gor Here");
                        console.log(data);
                        if (data.id === 0) {

                            $scope.gotErrors = true;
                            if (data[0].exception === "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            $modalInstance.close(data);
                            alrt.msg = "Record has been Update successfully";
                            $modal.open(
                                {
                                    templateUrl: "PopUpModel.html",
                                    controller: "PopUpController", resolve: { message: function () { return alrt } }
                                });
                            modalInstance.result.then(function (selectedItem) {
                            },
                                function () {
                                    console.log("Cancel Condintion");
                                })
                        }
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }
        }


        var uploader1 = $scope.uploader1 = new FileUploader({

            url: serviceBase + 'api/logoUpload/UploadId_Proof?PeopleID=' + $scope.PeopleData.PeopleID
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

            url: serviceBase + 'api/logoUpload/UploadAddressProof?PeopleID=' + $scope.PeopleData.PeopleID
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

            url: serviceBase + 'api/logoUpload/UploadMarksheet?PeopleID=' + $scope.PeopleData.PeopleID

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
        var uploader4 = $scope.uploader4 = new FileUploader({

            url: serviceBase + 'api/logoUpload/UploadPreSalary?PeopleID=' + $scope.PeopleData.PeopleID

        });
        //FILTERS

        uploader4.filters.push({

            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader4.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader4.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
        };
        uploader4.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader4.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader4.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader4.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader4.onSuccessItem = function (fileItem, response, status, headers) {
            console.info('onSuccessItem', fileItem, response, status, headers);
        };
        uploader4.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader4.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
            alert("Image Uploaded Failed");

        };
        uploader4.onCompleteItem = function (fileItem, response, status, headers) {
            $scope.uploadedfileName = fileItem._file.name;
            alert("Image Uploaded Successfully");

        };
        uploader4.onCompleteAll = function () {
            console.info('onCompleteAll');
        };
        //#endregion
    }
})(); ////end

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeletepeople', ModalInstanceCtrldeletepeople);

    ModalInstanceCtrldeletepeople.$inject = ["$scope", '$http', '$modal', "$modalInstance", "peoplesService", 'ngAuthSettings', "people"];

    function ModalInstanceCtrldeletepeople($scope, $http, $modal, $modalInstance, peoplesService, ngAuthSettings, people) {
        console.log("delete modal opened");
        var alrt = {};
        if (people) {
            $scope.PeopleData = people;
            console.log("found people");
            console.log(people);
            console.log($scope.PeopleData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deletepeoples = function (dataToPost) {

            console.log("Delete people controller");


            peoplesService.deletepeoplesdata(dataToPost).then(function (results) {

                console.log("Del");
                $modalInstance.close(dataToPost);
                alrt.msg = "Entry Deleted";


                $scope.AddData = function () {

                    var url = serviceBase + "api/trackuser?action=Delete&item=PeopleID:" + dataToPost.PeopleID;
                    $http.post(url).success(function (results) {

                    });

                }

                $scope.AddData();
                $modal.open(
                    {
                        templateUrl: "PopUpModel.html",
                        controller: "PopUpController", resolve: { message: function () { return alrt } }
                    });
                modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
            }, function (error) {
                alert(error);
            });
        }


    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlPeopleSecond', ModalInstanceCtrlPeopleSecond);

    ModalInstanceCtrlPeopleSecond.$inject = ["$scope", '$http', "designationservice", "DepartmentService", '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', "$modalInstance", "people1", 'WarehouseService', 'authService'];

    function ModalInstanceCtrlPeopleSecond($scope, $http, designationservice, DepartmentService, $modal, ngAuthSettings, peoplesService, CityService, StateService, $modalInstance, people1, WarehouseService, authService) {



        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("People");

        $scope.PeopleData = {}; var alrt = {};
        if (people1) {
            $scope.PeopleData = people1;
        }



        $scope.Salary_Calculation = function (data) {


            $scope.PeopleData.Hra_Salary = (parseInt(data) * 40) / 100;
            //$scope.PeopleData.Lta_Salary = parseInt(data) + 1000;
            //$scope.PeopleData.CA_Salary = parseInt(data) + 1000;
            //$scope.PeopleData.DA_Salary = parseInt(data) + 1000;
            $scope.PeopleData.PF_Salary = (((parseInt(data) * 75) / 100) * 12) / 100;
            $scope.PeopleData.ESI_Salary = (parseInt(data) * 6.5) / 100;
            //$scope.PeopleData.B_Salary = parseInt(data) + 1000;

        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.AddPeople = function (PeopleData) {



            authService.saveRegistrationpeople($scope.PeopleData).then(function (response) {
                console.log(response);
                $modalInstance.close(response);
                $scope.page = response.config.data.Department;
                if (response.status === "200") {

                    $scope.AddData = function () {

                        var url = serviceBase + "api/trackuser?action=AddPeople&item=" + $scope.page;
                        $http.post(url).success(function (results) {

                        });
                    }

                    $scope.AddData();

                    alrt.msg = "Record has been Save successfully";
                    $modal.open(
                        {
                            templateUrl: "PopUpModel.html",
                            controller: "PopUpController", resolve: { message: function () { return alrt } }
                        });
                    modalInstance.result.then(function (selectedItem) {
                    },
                        function () {
                            console.log("Cancel Condintion");
                        })
                }
            });
        };
        $scope.Designation = [];
        designationservice.getdesignations().then(function (results) {
            $scope.Designation = results.data;
        }, function (error) {
        });

        $scope.Departments = [];
        DepartmentService.getdepartments().then(function (results) {
            $scope.Departments = results.data;
        }, function (error) {
        });
        $scope.PutPeople = function (data) {
            
            if (data.PeopleFirstName === null || data.PeopleFirstName === "") {
                alert('Please Enter First Name');
                $modalInstance.open();
            }
            else if (data.PeopleLastName === null || data.PeopleLastName === "") {
                alert('Please Enter Last Name');
                $modalInstance.open();
            }
            else if (data.Email === null || data.Email === "") {
                alert('Please Enter Email');
                $modalInstance.open();
            }
            else if (data.Password === null || data.Password === "") {
                alert('Please Enter Password');
                $modalInstance.open();
            }
            else if (data.Mobile === null || data.Mobile === "") {
                alert('Please Enter Mobile');
                $modalInstance.open();
            }
            else if (data.Stateid === "") {
                alert('Please Enter State');
                $modalInstance.open();
            }
            else if (data.Cityid === "") {
                alert('Please Enter City');
                $modalInstance.open();
            }
            else if (data.WarehouseId === "") {
                alert('Please Enter Warehouse');
                $modalInstance.open();
            }
            else if (data.Department === "") {
                alert('Please Enter Department');
                $modalInstance.open();
            }
            $scope.PeopleData = {};
            if (people) {
                $scope.PeopleData = people;
                console.log("found Puttt People");
                console.log(people);
                console.log($scope.PeopleData);
                $scope.ok = function () { $modalInstance.close(); };
                $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                console.log("Update People");


                $scope.AddData = function () {

                    var url = serviceBase + "api/trackuser?action=Edit&item=PeopleID:" + $scope.PeopleData.PeopleID;
                    $http.post(url).success(function (results) {

                    });

                }
                $scope.AddData();

                var url = serviceBase + "/api/Peoples";
                var dataToPost = {
                    PeopleID: $scope.PeopleData.PeopleID, Password: $scope.PeopleData.Password,
                    Active: $scope.PeopleData.Active,
                    Stateid: $scope.PeopleData.Stateid, Cityid: $scope.PeopleData.Cityid,
                    Mobile: $scope.PeopleData.Mobile,
                    PeopleFirstName: $scope.PeopleData.PeopleFirstName,
                    PeopleLastName: $scope.PeopleData.PeopleLastName,
                    WarehouseId: $scope.PeopleData.WarehouseId,
                    CreatedDate: $scope.PeopleData.CreatedDate,
                    UpdatedDate: $scope.PeopleData.UpdatedDate,
                    CreatedBy: $scope.PeopleData.CreatedBy,
                    UpdateBy: $scope.PeopleData.UpdateBy,
                    Email: $scope.PeopleData.Email,
                    Department: $scope.PeopleData.Department,
                    BillableRate: $scope.PeopleData.BillableRate,
                    CostRate: $scope.PeopleData.CostRate,
                    Permissions: $scope.PeopleData.Permissions,
                    Skcode: $scope.PeopleData.Skcode,
                    Type: $scope.PeopleData.Department,
                    Salesexecutivetype: $scope.PeopleData.Salesexecutivetype,
                    AgentCode: $scope.PeopleData.AgentCode,
                    ImageUrl: data.ImageUrl
                };
                console.log(dataToPost);
                $http.put(url)
                    .success(function (data) {
                        console.log("Error Gor Here");
                        console.log(data);
                        if (data.id === 0) {

                            $scope.gotErrors = true;
                            if (data[0].exception === "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            $modalInstance.close(data);
                            alrt.msg = "Record has been Update successfully";
                            $modal.open(
                                {
                                    templateUrl: "PopUpModel.html",
                                    controller: "PopUpController", resolve: { message: function () { return alrt } }
                                });
                            modalInstance.result.then(function (selectedItem) {
                            },
                                function () {
                                    console.log("Cancel Condintion");
                                })
                        }
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PeopleimagesController', PeopleimagesController);

    PeopleimagesController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "NewsService", "FileUploader", "unitMasterService", "localStorageService", "$modal"];

    function PeopleimagesController($scope, $http, ngAuthSettings, $modalInstance, object, ItemService, FileUploader, unitMasterService, localStorageService, $modal) {

        if (object) {
            $scope.CustomerDataOnject = object;
        }
        $scope.baseurl = serviceBase + "/UploadDocuments/";


        console.log('$scope.PeopleID:', $scope.PeopleID);

        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.dataPeopleImage = [];
        var url = serviceBase + "api/Peoples/PeopleImages?PeopleId=" + $scope.CustomerDataOnject.PeopleID;
        $http.get(url).success(function (response) {
            $scope.dataPeopleImage = response;
            console.log($scope.dataPeopleImage);

        });
    }
})();

(function () {
        'use strict';

        angular
            .module('app')
            .controller('ModalInstanceChange', ModalInstanceChange);

    ModalInstanceChange.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "people", 'FileUploader'];

    function ModalInstanceChange($scope, $http, ngAuthSettings, $modalInstance, people, FileUploader) {

        $scope.data = people;

        $scope.PeopleData = {

        };

        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

            $scope.Update = function (password, confirmpassword) {
                
                if (people) {
                    $scope.PeopleData = people;
                    console.log("found Puttt People");
                    console.log(people);
                    console.log($scope.PeopleData);
                    $scope.ok = function () { $modalInstance.close(); };
                    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
                    console.log("Update People");

                }

                if (password != confirmpassword || password == undefined && confirmpassword == undefined || password == "" && confirmpassword == "") {
                    alert("Passwords do not match.");
                    return false;
                }
                if (password.length < 6 && password.length < 6) {
                    alert("Passwords length atleast 6 digit.");
                    return false;
                }

                var url = serviceBase + "api/Account/ChangePeoplePassword";
                var dataToPost =
                {
                    Email: $scope.PeopleData.Email,
                    Password: password
                }

                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {
                        $modalInstance.close(data);
                        window.location.reload();
                        console.log("save data");

                        alert('Password changed Successfuly');



                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);

                    })


            };
        }
    })();

